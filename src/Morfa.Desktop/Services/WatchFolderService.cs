// Runs the watch folders of the settings (Settings → Watch folders) while Morfa is in the tray: one FolderWatcher
// per enabled rule, rebuilt shortly after the rules change. Conversions go through ConversionService, so they show in
// the toast and the history like any other.

using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Morfa.Domain.Settings;
using Morfa.Domain.Watching;

namespace Morfa.Desktop.Services;

public sealed class WatchFolderService(UserDataStore store, ConversionService conversions, ILogger<WatchFolderService> log)
    : IAsyncDisposable
{
    /// <summary>Typing a folder path saves on every key: wait for a pause before (re)starting watchers.</summary>
    private static readonly TimeSpan ApplyDelay = TimeSpan.FromSeconds(1.5);

    private readonly Dictionary<string, (string Signature, FolderWatcher Watcher)> _running = [];
    private readonly Dictionary<string, string?> _errors = [];
    private readonly SemaphoreSlim _applySync = new(1, 1); // serializes ApplyAsync so a timer fire during an apply can't double-start a watcher
    private IDisposable? _pendingApply;

    /// <summary>Raised (UI thread) when a rule started, stopped or failed to start.</summary>
    public event EventHandler? StatusChanged;

    public void Start()
    {
        conversions.WatchRetry = RetryAsync;
        _ = ApplyAsync();
        store.SettingsChanged += (_, _) =>
        {
            _pendingApply?.Dispose();
            _pendingApply = DispatcherTimer.RunOnce(() => _ = ApplyAsync(), ApplyDelay);
        };
    }

    /// <summary>True when the rule's folder is being watched.</summary>
    public bool IsWatching(string ruleId) => _running.ContainsKey(ruleId);

    /// <summary>Why an enabled rule is not being watched (folder missing, …), or null.</summary>
    public string? ErrorOf(string ruleId) => _errors.GetValueOrDefault(ruleId);

    /// <summary>Starts, restarts and stops watchers so they match the settings.</summary>
    private async Task ApplyAsync()
    {
        await _applySync.WaitAsync();
        try
        {
            var wanted = store.Settings.WatchFolders
                .Where(r => r.Enabled && !string.IsNullOrWhiteSpace(r.Folder)
                    && (r.Routes.Count > 0 || !string.IsNullOrWhiteSpace(r.PresetId)))
                .ToDictionary(r => r.Id, r => r.Clone());

            foreach (var (id, (signature, watcher)) in _running.ToList())
            {
                if (wanted.TryGetValue(id, out var rule) && Signature(rule) == signature)
                    continue;
                _running.Remove(id);
                await watcher.DisposeAsync();
            }
            _errors.Clear();
            foreach (var (id, rule) in wanted)
            {
                if (_running.ContainsKey(id))
                    continue;
                var watcher = new FolderWatcher(rule, (files, ct) => ConvertAsync(rule, files, ct), log);
                if (watcher.Start())
                {
                    _running[id] = (Signature(rule), watcher);
                    log.LogInformation("Watching {Folder}", rule.Folder);
                }
                else
                {
                    _errors[id] = watcher.Error;
                    await watcher.DisposeAsync();
                }
            }
            Dispatcher.UIThread.Post(() => StatusChanged?.Invoke(this, EventArgs.Empty));
        }
        finally
        {
            _applySync.Release();
        }
    }

    private async Task<Domain.Conversion.ConversionJob?> ConvertAsync(WatchRule rule, IReadOnlyList<string> files, CancellationToken cancellationToken)
    {
        if (rule.Routes.Count == 0)
        {
            var preset = store.FindConversion(rule.PresetId);
            if (preset is null)
            {
                log.LogWarning("Watch folder {Folder}: preset {Preset} no longer exists", rule.Folder, rule.PresetId);
                return null;
            }
            return await conversions.RunAsync(files, rule.Apply(preset), rule.Id, cancellationToken);
        }

        // Conditional routing: one conversion per chosen preset, combined into a single result for the watcher.
        var jobs = new List<Domain.Conversion.ConversionJob>();
        var submitted = new List<string>();
        foreach (var group in files.GroupBy(f => rule.PresetIdFor(f) ?? "").Where(g => g.Key.Length > 0))
        {
            var groupFiles = group.ToList();
            var preset = store.FindConversion(group.Key);
            if (preset is null)
            {
                log.LogWarning("Watch folder {Folder}: preset {Preset} no longer exists", rule.Folder, group.Key);
                continue;
            }
            if (await conversions.RunAsync(groupFiles, rule.Apply(preset), rule.Id, cancellationToken) is { } job)
            {
                jobs.Add(job);
                submitted.AddRange(groupFiles);
            }
        }
        foreach (var unmatched in files.Except(submitted, StringComparer.OrdinalIgnoreCase))
            log.LogWarning("Watch folder {Folder}: no route for {Path}; skipping it", rule.Folder, unmatched);
        if (jobs.Count == 0)
            return null;
        var combined = new Domain.Conversion.ConversionJob(submitted, jobs[0].Preset, jobs[0].PresetDisplayName);
        foreach (var source in combined.Files)
        {
            var from = jobs.SelectMany(j => j.Files).FirstOrDefault(f =>
                string.Equals(f.SourcePath, source.SourcePath, StringComparison.OrdinalIgnoreCase));
            if (from is null) continue;
            source.State = from.State;
            source.Progress = from.Progress;
            source.Outputs.AddRange(from.Outputs);
            source.ErrorKey = from.ErrorKey;
            source.ErrorDetail = from.ErrorDetail;
        }
        return combined;
    }

    /// <summary>
    /// Converts files of a watch folder again (a retry from the toast or the history) with the preset as it ran, then
    /// moves the originals that converted when the folder does that.
    /// </summary>
    internal async Task RetryAsync(string ruleId, IReadOnlyList<string> files, Domain.Presets.Preset preset)
    {
        var job = await conversions.RunAsync(files, preset, ruleId);
        if (job is null || store.Settings.WatchFolders.FirstOrDefault(r => r.Id == ruleId) is not { Originals: AfterConversion.MoveToOriginals } rule)
            return;
        foreach (var file in job.Files.Where(f => f.State == Domain.Conversion.FileState.Done))
            FolderWatcher.MoveToOriginals(rule, file.SourcePath, log);
    }

    /// <summary>Everything that needs a new watcher when it changes.</summary>
    private static string Signature(WatchRule rule) =>
        string.Join('|', rule.Folder, rule.PresetId, rule.IncludeSubfolders, rule.OutputFolder, rule.Originals,
            string.Join('/', rule.Include), rule.FileNamePattern,
            string.Join('/', rule.Renames.Select(r => $"{r.Kind}:{r.Find}>{r.Replace}")),
            rule.RetentionDays,
            string.Join('/', rule.Routes.Select(r =>
                $"{r.PresetId}:{string.Join(',', r.NamePatterns)}:{string.Join(',', r.Formats)}:{r.MinSizeMb}:{r.MaxSizeMb}")));

    public async ValueTask DisposeAsync()
    {
        foreach (var (_, watcher) in _running.Values)
            await watcher.DisposeAsync();
        _running.Clear();
    }
}
