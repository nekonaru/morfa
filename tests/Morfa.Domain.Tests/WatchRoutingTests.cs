// F3 domain tests: conditional routing, persistent failure suppression and retention.

using Morfa.Domain.Conversion;
using Morfa.Domain.Presets;
using Morfa.Domain.Watching;

namespace Morfa.Domain.Tests;

public class WatchRoutingTests
{
    [Fact]
    public void The_first_matching_route_wins()
    {
        using var dir = new TempDir();
        var jpg = dir.File("photo.jpg");
        var pdf = dir.File("report.pdf");
        var rule = new WatchRule
        {
            Folder = dir.Path,
            PresetId = "default",
            Routes =
            [
                new WatchRoute { PresetId = "pdf-route", Formats = ["pdf"] },
                new WatchRoute { PresetId = "image-route", NamePatterns = ["*.jpg"], Formats = ["jpg", "png"] },
            ],
        };

        Assert.Equal("pdf-route", rule.PresetIdFor(pdf));
        Assert.Equal("image-route", rule.PresetIdFor(jpg));
    }

    [Fact]
    public void A_rule_without_routes_keeps_its_single_preset()
    {
        using var dir = new TempDir();
        var rule = new WatchRule { Folder = dir.Path, PresetId = "only" };

        Assert.Equal("only", rule.PresetIdFor(dir.File("a.jpg")));
    }

    [Fact]
    public void A_suppressed_file_stays_suppressed_after_a_restart()
    {
        using var dir = new TempDir();
        var rule = new WatchRule { Id = "watch-1", Folder = dir.Path };
        var file = dir.File("corrupt.jpg");
        var first = new GeneratedWatchOutputs(rule);
        first.Suppress(file);
        Assert.True(first.IsSuppressed(file));

        var restarted = new GeneratedWatchOutputs(rule);
        restarted.Load();
        Assert.True(restarted.IsSuppressed(file));
    }

    [Fact]
    public void Old_outputs_are_deleted_when_retention_is_set()
    {
        using var dir = new TempDir();
        var outputFolder = Path.Combine(dir.Path, WatchRule.DefaultOutputName);
        Directory.CreateDirectory(outputFolder);
        var old = Path.Combine(outputFolder, "old.png");
        var recent = Path.Combine(outputFolder, "recent.png");
        File.WriteAllText(old, "x");
        File.WriteAllText(recent, "x");
        File.SetLastWriteTimeUtc(old, DateTime.UtcNow.AddDays(-10));
        File.SetLastWriteTimeUtc(recent, DateTime.UtcNow);
        var rule = new WatchRule { Folder = dir.Path, RetentionDays = 2 };

        var watcher = new FolderWatcher(rule, (_, _) => Task.FromResult<ConversionJob?>(null));
        watcher.ApplyRetention();

        Assert.False(File.Exists(old));
        Assert.True(File.Exists(recent));
    }
}
