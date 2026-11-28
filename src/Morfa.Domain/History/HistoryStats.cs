// Aggregates used by the Home dashboard. Pure logic, no IO.

using Morfa.Domain.Conversion;

namespace Morfa.Domain.History;

public sealed record HistoryStats(
    int Total,
    int Succeeded,
    int Failed,
    double SuccessRate,
    long BytesIn,
    long BytesOut,
    string? TopFormat,
    string? TopPreset,
    int ThisWeek)
{
    public static HistoryStats Of(IEnumerable<HistoryEntry> entries)
    {
        var list = entries.ToList();
        var succeeded = list.Count(e => e.State is JobState.Completed or JobState.CompletedWithErrors);
        var failed = list.Count(e => e.State is JobState.Failed or JobState.Cancelled);
        var week = DateTime.Now.AddDays(-7);
        var formats = list
            .SelectMany(e => e.Outputs)
            .Select(path => System.IO.Path.GetExtension(path).TrimStart('.').ToLowerInvariant())
            .Where(ext => ext.Length > 0)
            .GroupBy(ext => ext)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();
        var presets = list
            .Where(e => !string.IsNullOrWhiteSpace(e.PresetName))
            .GroupBy(e => e.PresetName!)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();
        return new HistoryStats(
            list.Count,
            succeeded,
            failed,
            list.Count == 0 ? 0 : (double)succeeded / list.Count,
            list.Sum(e => e.BytesIn),
            list.Sum(e => e.BytesOut),
            formats,
            presets,
            list.Count(e => e.FinishedAt >= week));
    }
}
