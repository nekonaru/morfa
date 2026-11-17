// Exports the conversion history as a UTF-8 CSV file (date,preset,state,sources,outputs,failures).

using System.Text;
using Morfa.Domain.History;

namespace Morfa.Desktop.Services;

internal static class HistoryCsv
{
    /// <summary>Writes the entries as CSV, one row per entry. Multi-value cells are separated by semicolons.</summary>
    public static void Write(string path, IReadOnlyList<HistoryEntry> entries)
    {
        var sb = new StringBuilder();
        sb.AppendLine("FinishedAt,Preset,State,Sources,Outputs,Failures");
        foreach (var entry in entries)
        {
            var failures = entry.Failures
                .Select(f => $"{f.Source} = {f.ErrorKey}: {f.ErrorDetail}")
                .ToList();
            var cells = new[]
            {
                entry.FinishedAt.ToString("o", System.Globalization.CultureInfo.InvariantCulture),
                entry.PresetName,
                entry.State.ToString(),
                string.Join(';', entry.Sources),
                string.Join(';', entry.Outputs),
                string.Join(';', failures),
            };
            sb.Append(string.Join(',', cells.Select(CsvCell))).AppendLine();
        }
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    private static string CsvCell(string value)
    {
        if (value.AsSpan().IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0)
            return value;
        return "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }
}
