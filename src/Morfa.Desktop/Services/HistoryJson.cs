// Exports the conversion history as UTF-8 JSON, with the same information as the CSV export.

using System.Text.Json;
using Morfa.Domain.History;

namespace Morfa.Desktop.Services;

internal static class HistoryJson
{
    public static void Write(string path, IReadOnlyList<HistoryEntry> entries)
    {
        var rows = entries.Select(entry => new
        {
            date = entry.FinishedAt.ToString("o", System.Globalization.CultureInfo.InvariantCulture),
            preset = entry.PresetName,
            state = entry.State.ToString(),
            sources = entry.Sources,
            outputs = entry.Outputs,
            failures = entry.Failures.Select(f => new
            {
                source = f.Source,
                error = f.ErrorKey,
                detail = f.ErrorDetail,
            }).ToList(),
            pinned = entry.Pinned,
            tags = entry.Tags,
            notes = entry.Notes,
        }).ToList();
        File.WriteAllText(path, JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true }));
    }
}
