// Remembers generated results when a watch rule writes back into its input tree. Without this, restarting
// an inbox watcher would convert yesterday's outputs again. Entries identify a file version, not just a name:
// replacing an output with a new incoming file makes it eligible again.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Morfa.Domain.Platform;

namespace Morfa.Domain.Watching;

internal sealed class GeneratedWatchOutputs(WatchRule rule)
{
    internal sealed record Entry(string Path, long Length, long LastWriteTicks, bool IsDirectory = false);

    private readonly Dictionary<string, Entry> _entries = new(FileSystemPaths.Comparer);
    private readonly HashSet<string> _directories = new(FileSystemPaths.Comparer);
    private readonly HashSet<string> _suppressed = new(StringComparer.Ordinal);
    private readonly HashSet<string> _retention = new(FileSystemPaths.Comparer);
    private readonly object _lock = new();
    internal sealed record State(List<Entry> Entries, List<string> Suppressed, List<string> Retention);
    private string StateFile => System.IO.Path.Combine(rule.Folder, ".morfa-watch-" +
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rule.Id)))[..16].ToLowerInvariant() + ".json");

    public void Load()
    {
        if (!File.Exists(StateFile)) return;
        var text = File.ReadAllText(StateFile);
        using var document = JsonDocument.Parse(text);
        List<Entry?> entries;
        if (document.RootElement.ValueKind == JsonValueKind.Array)
        {
            entries = JsonSerializer.Deserialize<List<Entry?>>(text) ?? [];
        }
        else if (document.RootElement.ValueKind == JsonValueKind.Object)
        {
            entries = document.RootElement.TryGetProperty("entries", out var list)
                ? JsonSerializer.Deserialize<List<Entry?>>(list.GetRawText()) ?? []
                : document.RootElement.TryGetProperty("Entries", out var oldList)
                    ? JsonSerializer.Deserialize<List<Entry?>>(oldList.GetRawText()) ?? []
                    : [];
            var suppressedProperty = document.RootElement.TryGetProperty("suppressed", out var suppressed)
                ? suppressed
                : document.RootElement.TryGetProperty("Suppressed", out var oldSuppressed) ? oldSuppressed : default;
            if (suppressedProperty.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in suppressedProperty.EnumerateArray())
                    if (item.GetString() is { Length: > 0 } key) _suppressed.Add(key);
            }
            var retentionProperty = document.RootElement.TryGetProperty("retention", out var retention)
                ? retention
                : document.RootElement.TryGetProperty("Retention", out var oldRetention) ? oldRetention : default;
            if (retentionProperty.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in retentionProperty.EnumerateArray())
                    if (item.GetString() is { Length: > 0 } path && IsInside(path, rule.Folder)) _retention.Add(path);
            }
        }
        else
        {
            return; // a valid JSON scalar is not a state file; start fresh
        }
        foreach (var entry in entries)
        {
            if (entry is null || !IsCurrent(entry)) continue;
            _entries[entry.Path] = entry;
            if (entry.IsDirectory) _directories.Add(entry.Path);
        }
    }

    public bool Contains(string path)
    {
        if (_entries.TryGetValue(path, out var entry) && IsCurrent(entry)) return true;
        return _directories.Any(directory => IsInside(path, directory) && Directory.Exists(directory));
    }

    public bool Remember(string path)
    {
        if (!IsInside(path, rule.Folder)) return false;
        if (Directory.Exists(path))
        {
            _entries[path] = new(path, 0, 0, true);
            _directories.Add(path);
        }
        else
        {
            var info = new FileInfo(path);
            if (!info.Exists) return false;
            _entries[path] = new(path, info.Length, info.LastWriteTimeUtc.Ticks);
        }
        return true;
    }

    /// <summary>Remembers that this exact file failed too often, so a restart does not retry it forever.</summary>
    public void Suppress(string path)
    {
        if (!IsInside(path, rule.Folder)) return;
        var info = new FileInfo(path);
        if (!info.Exists) return;
        lock (_lock)
            _suppressed.Add($"{path}|{info.Length}|{info.LastWriteTimeUtc.Ticks}");
        try { Save(); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }

    public bool IsSuppressed(string path)
    {
        if (!File.Exists(path)) return false;
        var info = new FileInfo(path);
        lock (_lock)
            return _suppressed.Contains($"{path}|{info.Length}|{info.LastWriteTimeUtc.Ticks}");
    }

    /// <summary>Outputs this rule created, used by retention so it never deletes unrelated user files.</summary>
    public IReadOnlyList<string> RetentionPaths()
    {
        lock (_lock)
            return [.. _retention];
    }

    public void RememberForRetention(string path)
    {
        if (!IsInside(path, rule.Folder) || !File.Exists(path)) return;
        lock (_lock)
            _retention.Add(path);
    }

    public void Forget(string path)
    {
        lock (_lock)
            _retention.Remove(path);
    }

    public void Save()
    {
        lock (_lock)
        {
            foreach (var stale in _entries.Values.Where(e => !IsCurrent(e)).ToArray())
            {
                _entries.Remove(stale.Path);
                _directories.Remove(stale.Path);
            }
            foreach (var key in _suppressed.Where(k => !File.Exists(k.Split('|')[0])).ToArray())
                _suppressed.Remove(key);
            foreach (var path in _retention.Where(p => !File.Exists(p)).ToArray())
                _retention.Remove(path);
        }
        var temporary = StateFile + "." + Guid.NewGuid().ToString("N")[..8] + ".tmp";
        try
        {
            string json;
            lock (_lock)
            {
                json = JsonSerializer.Serialize(new State([.. _entries.Values], [.. _suppressed], [.. _retention]));
            }
            File.WriteAllText(temporary, json);
            File.Move(temporary, StateFile, overwrite: true);
            if (OperatingSystem.IsWindows())
                File.SetAttributes(StateFile, File.GetAttributes(StateFile) | FileAttributes.Hidden);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private bool IsCurrent(Entry entry)
    {
        // A copied or edited state file cannot exclude the root itself or files outside this rule.
        if (string.IsNullOrWhiteSpace(entry.Path) || !IsInside(entry.Path, rule.Folder)) return false;
        if (entry.IsDirectory) return Directory.Exists(entry.Path);
        var info = new FileInfo(entry.Path);
        return info.Exists && info.Length == entry.Length && info.LastWriteTimeUtc.Ticks == entry.LastWriteTicks;
    }

    private static bool IsInside(string path, string folder) => System.IO.Path.GetFullPath(path).StartsWith(
        System.IO.Path.TrimEndingDirectorySeparator(System.IO.Path.GetFullPath(folder)) + System.IO.Path.DirectorySeparatorChar,
        FileSystemPaths.Comparison);
}
