// Persistent conversion queue: job metadata only, never file contents. The file is atomic like settings.json.

using System.Text.Json;
using Morfa.Domain.Conversion;
using Morfa.Domain.Presets;
using Morfa.Domain.Settings;

namespace Morfa.Domain.Queue;

/// <summary>The on-disk form of the queue. Version 2 is the first format.</summary>
public sealed record QueueFile
{
    public const int CurrentVersion = 2;

    public int Version { get; set; } = CurrentVersion;
    public List<JobSnapshot> Jobs { get; set; } = [];
}

/// <summary>One job as saved between runs. <see cref="Preset"/> holds serialized preset JSON.</summary>
public sealed record JobSnapshot
{
    public string Id { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public string State { get; set; } = nameof(JobState.Queued);

    /// <summary>A paused job is not restored into the running queue until it is resumed.</summary>
    public bool Paused { get; set; }
    public int Priority { get; set; }
    public string PresetDisplayName { get; set; } = "";
    public string Preset { get; set; } = "";
    public List<string> Sources { get; set; } = [];
    public string? WatchRuleId { get; set; }
    public int Attempts { get; set; }
    public int MaxAttempts { get; set; } = 3;
    public List<string> Outputs { get; set; } = [];
    public List<FileSnapshot> Files { get; set; } = [];

    /// <summary>Takes a snapshot. The caller owns the job and may keep changing it afterwards.</summary>
    public static JobSnapshot From(ConversionJob job, int attempts = 0, int maxAttempts = 3, string? watchRuleId = null) =>
        new()
        {
            Id = job.Id,
            CreatedAt = job.CreatedAt,
            State = job.State.ToString(),
            Priority = 0,
            PresetDisplayName = job.PresetDisplayName,
            Preset = JsonSerializer.Serialize(job.Preset, MorfaJsonContext.Default.Preset),
            Sources = [.. job.Sources],
            WatchRuleId = watchRuleId,
            Attempts = attempts,
            MaxAttempts = maxAttempts,
            Outputs = [.. job.Outputs],
            Files = [.. job.Files.Select(f => new FileSnapshot
            {
                SourcePath = f.SourcePath,
                State = f.State.ToString(),
                Progress = f.Progress,
                ErrorKey = f.ErrorKey,
                ErrorDetail = f.ErrorDetail,
                Outputs = [.. f.Outputs],
            })],
        };

    /// <summary>Rebuilds a job. A job that was running comes back as queued, so no work is lost or half trusted.</summary>
    public ConversionJob ToJob()
    {
        var preset = JsonSerializer.Deserialize(Preset, MorfaJsonContext.Default.Preset)
            ?? throw new InvalidDataException($"Queue job {Id} has no readable preset.");
        var job = new ConversionJob(Sources, preset, PresetDisplayName)
        {
            Id = Id,
            CreatedAt = CreatedAt,
        };
        for (var i = 0; i < Files.Count && i < job.Files.Count; i++)
        {
            var source = Files[i];
            var file = job.Files[i];
            if (!string.Equals(file.SourcePath, source.SourcePath, StringComparison.Ordinal))
                throw new InvalidDataException($"Queue job {Id} changed its sources on disk.");
            file.State = Enum.TryParse<FileState>(source.State, out var state) ? state : FileState.Pending;
            file.Progress = source.Progress;
            file.ErrorKey = source.ErrorKey;
            file.ErrorDetail = source.ErrorDetail;
            file.Outputs.AddRange(source.Outputs);
        }
        job.State = Enum.TryParse<JobState>(State, out var jobState) && jobState is not JobState.Running
            ? jobState
            : JobState.Queued;
        return job;
    }
}

/// <summary>One source file inside a saved job.</summary>
public sealed record FileSnapshot
{
    public string SourcePath { get; set; } = "";
    public string State { get; set; } = nameof(FileState.Pending);
    public double Progress { get; set; }
    public string? ErrorKey { get; set; }
    public string? ErrorDetail { get; set; }
    public List<string> Outputs { get; set; } = [];
}

/// <summary>Reads and writes the queue file with temp-plus-rename, like UserDataStore.</summary>
public sealed class JobStore(string? folder = null)
{
    private readonly object _lock = new();
    private readonly string _folder = folder ?? UserDataStore.DefaultDirectory;

    public string FilePath => Path.Combine(_folder, ".queue-v2.json");

    /// <summary>When off, reads still work but nothing is written. Tests use this to stay off the user's data.</summary>
    public bool Enabled { get; set; } = true;

    public QueueFile Load()
    {
        lock (_lock)
        {
            if (!File.Exists(FilePath))
                return new QueueFile();
            try
            {
                using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
                return JsonSerializer.Deserialize(stream, MorfaJsonContext.Default.QueueFile) ?? new QueueFile();
            }
            catch (Exception ex) when (ex is JsonException or IOException or NotSupportedException or UnauthorizedAccessException)
            {
                try { File.Copy(FilePath, FilePath + ".broken", overwrite: true); } catch (IOException) { }
                return new QueueFile();
            }
        }
    }

    public void Save(QueueFile file)
    {
        if (!Enabled)
            return;
        lock (_lock)
        {
            System.IO.Directory.CreateDirectory(_folder);
            var temp = FilePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var stream = File.Create(temp))
                    JsonSerializer.Serialize(stream, file, MorfaJsonContext.Default.QueueFile);
                for (var attempt = 0; ; attempt++)
                {
                    try { File.Move(temp, FilePath, overwrite: true); break; }
                    catch (IOException) when (attempt < 3) { Thread.Sleep(50); }
                }
            }
            finally
            {
                try { File.Delete(temp); } catch (IOException) { }
            }
        }
    }

    public void Upsert(JobSnapshot job)
    {
        var file = Load();
        var index = file.Jobs.FindIndex(j => j.Id == job.Id);
        if (index < 0)
            file.Jobs.Add(job);
        else
            file.Jobs[index] = job;
        Save(file);
    }

    public void Remove(string id)
    {
        var file = Load();
        if (file.Jobs.RemoveAll(j => j.Id == id) > 0)
            Save(file);
    }

    public void Clear() => Save(new QueueFile());
}
