// F3 domain tests: history metadata, dashboard numbers and watch routing.

using Morfa.Domain.Conversion;
using Morfa.Domain.History;
using Morfa.Domain.Presets;
using Morfa.Domain.Settings;
using Morfa.Domain.Watching;

namespace Morfa.Domain.Tests;

public class HistoryV2Tests
{
    [Fact]
    public void Pin_tags_and_notes_round_trip_through_history_json()
    {
        using var dir = new TempDir();
        var store = new UserDataStore(dir.Path);
        store.Load();
        store.AddHistory(new HistoryEntry
        {
            JobId = "job-1",
            PresetId = "to-pdf",
            PresetName = "To PDF",
            State = JobState.Completed,
            Sources = ["a.jpg"],
            Outputs = ["a.pdf"],
        });

        store.History[0].Pinned = true;
        store.History[0].Tags = ["client", "urgent"];
        store.History[0].Notes = "Send on Friday";
        store.SaveHistory();

        var reloaded = new UserDataStore(dir.Path);
        reloaded.Load();
        var entry = Assert.Single(reloaded.History);
        Assert.True(entry.Pinned);
        Assert.Equal(["client", "urgent"], entry.Tags);
        Assert.Equal("Send on Friday", entry.Notes);
    }

    [Fact]
    public void Dashboard_counts_match_the_entries()
    {
        var entries = new[]
        {
            new HistoryEntry { State = JobState.Completed, PresetName = "PNG", Outputs = ["a.png"], BytesIn = 100, BytesOut = 50, FinishedAt = DateTime.Now },
            new HistoryEntry { State = JobState.CompletedWithErrors, PresetName = "PNG", Outputs = ["b.png"], BytesIn = 200, BytesOut = 60, FinishedAt = DateTime.Now },
            new HistoryEntry { State = JobState.Failed, PresetName = "PDF", Outputs = [], BytesIn = 300, BytesOut = 0, FinishedAt = DateTime.Now.AddDays(-10) },
        };

        var stats = HistoryStats.Of(entries);

        Assert.Equal(3, stats.Total);
        Assert.Equal(2, stats.Succeeded);
        Assert.Equal(1, stats.Failed);
        Assert.Equal(2.0 / 3, stats.SuccessRate, 3);
        Assert.Equal(600, stats.BytesIn);
        Assert.Equal(110, stats.BytesOut);
        Assert.Equal("png", stats.TopFormat);
        Assert.Equal("PNG", stats.TopPreset);
        Assert.Equal(2, stats.ThisWeek);
    }
}
