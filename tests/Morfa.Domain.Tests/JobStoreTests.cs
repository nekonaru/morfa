// Round-trip tests for the v2 persistent queue file.

using Morfa.Domain.Conversion;
using Morfa.Domain.Presets;
using Morfa.Domain.Queue;

namespace Morfa.Domain.Tests;

public class JobStoreTests
{
    [Fact]
    public void Queue_file_round_trip_keeps_job_metadata_and_file_states()
    {
        using var dir = new TempDir();
        var store = new JobStore(dir.Path);
        var job = new ConversionJob([dir.File("a.jpg"), dir.File("b.png")],
            new Preset { TargetFormat = "webp" }, "WEBP");
        job.State = JobState.CompletedWithErrors;
        job.Files[0].State = FileState.Done;
        job.Files[0].Outputs.Add("out.webp");
        job.Files[1].State = FileState.Failed;
        job.Files[1].ErrorKey = "error.conversion_failed";
        job.Files[1].ErrorDetail = "engine stopped";

        store.Upsert(JobSnapshot.From(job, attempts: 2, maxAttempts: 5, watchRuleId: "watch-1"));

        var file = store.Load();
        var saved = Assert.Single(file.Jobs);
        Assert.Equal(2, file.Version);
        Assert.Equal(job.Id, saved.Id);
        Assert.Equal(job.CreatedAt, saved.CreatedAt);
        Assert.Equal(nameof(JobState.CompletedWithErrors), saved.State);
        Assert.Equal("WEBP", saved.PresetDisplayName);
        Assert.Equal("webp", System.Text.Json.Nodes.JsonNode.Parse(saved.Preset)!["targetFormat"]?.GetValue<string>());
        Assert.Equal(2, saved.Attempts);
        Assert.Equal(5, saved.MaxAttempts);
        Assert.Equal("watch-1", saved.WatchRuleId);
        Assert.Equal(["out.webp"], saved.Files[0].Outputs);
        Assert.Equal("error.conversion_failed", saved.Files[1].ErrorKey);

        var rebuilt = saved.ToJob();
        Assert.Equal(job.Id, rebuilt.Id);
        Assert.Equal(job.CreatedAt, rebuilt.CreatedAt);
        Assert.Equal(JobState.CompletedWithErrors, rebuilt.State);
        Assert.Equal("webp", rebuilt.Preset.TargetFormat);
        Assert.Equal(FileState.Done, rebuilt.Files[0].State);
        Assert.Equal(["out.webp"], rebuilt.Files[0].Outputs);
        Assert.Equal(FileState.Failed, rebuilt.Files[1].State);
        Assert.Equal("engine stopped", rebuilt.Files[1].ErrorDetail);
    }

    [Fact]
    public void A_job_that_was_running_comes_back_as_queued_and_a_paused_job_stays_paused()
    {
        using var dir = new TempDir();
        var store = new JobStore(dir.Path);
        var running = new ConversionJob([dir.File("a.jpg")], new Preset { TargetFormat = "png" }, "PNG");
        var paused = new ConversionJob([dir.File("b.jpg")], new Preset { TargetFormat = "png" }, "PNG");
        running.State = JobState.Running;
        paused.State = JobState.Cancelled;

        var runningSnapshot = JobSnapshot.From(running);
        var pausedSnapshot = JobSnapshot.From(paused);
        pausedSnapshot.Paused = true;
        store.Upsert(runningSnapshot);
        store.Upsert(pausedSnapshot);

        var loaded = store.Load();
        Assert.Equal(2, loaded.Jobs.Count);
        Assert.Equal(JobState.Queued, loaded.Jobs[0].ToJob().State);
        Assert.Equal(JobState.Cancelled, loaded.Jobs[1].ToJob().State);
        Assert.True(loaded.Jobs[1].Paused);
    }

    [Fact]
    public void Removing_and_clearing_jobs_rewrites_the_file()
    {
        using var dir = new TempDir();
        var store = new JobStore(dir.Path);
        var first = JobSnapshot.From(new ConversionJob([dir.File("a.jpg")], new Preset { TargetFormat = "png" }, "PNG"));
        var second = JobSnapshot.From(new ConversionJob([dir.File("b.jpg")], new Preset { TargetFormat = "png" }, "PNG"));
        store.Upsert(first);
        store.Upsert(second);

        store.Remove(first.Id);
        Assert.Equal([second.Id], store.Load().Jobs.Select(j => j.Id));

        store.Clear();
        Assert.Empty(store.Load().Jobs);
        Assert.True(File.Exists(store.FilePath));
    }

    [Theory]
    [InlineData(JobState.Queued)]
    [InlineData(JobState.Running)]
    [InlineData(JobState.Completed)]
    [InlineData(JobState.CompletedWithErrors)]
    [InlineData(JobState.Failed)]
    [InlineData(JobState.Cancelled)]
    public void Every_job_state_round_trips_with_all_metadata(JobState state)
    {
        using var dir = new TempDir();
        var store = new JobStore(dir.Path);
        var source = dir.File("a.jpg");
        var job = new ConversionJob([source], new Preset { TargetFormat = "png" }, "PNG");
        job.State = state;
        job.Files[0].Outputs.Add("a.png");
        var snapshot = JobSnapshot.From(job, attempts: 2, maxAttempts: 5, watchRuleId: "watch-1");
        snapshot.Priority = 4;
        snapshot.RecipeId = "recipe-1";

        store.Upsert(snapshot);
        var saved = Assert.Single(store.Load().Jobs);
        Assert.Equal(snapshot.Id, saved.Id);
        Assert.Equal([source], saved.Sources);
        Assert.Equal(["a.png"], saved.Outputs);
        Assert.Equal(4, saved.Priority);
        Assert.Equal("recipe-1", saved.RecipeId);
        Assert.Equal("watch-1", saved.WatchRuleId);
        Assert.Equal(2, saved.Attempts);
        Assert.Equal(5, saved.MaxAttempts);

        var rebuilt = saved.ToJob();
        Assert.Equal(state == JobState.Running ? JobState.Queued : state, rebuilt.State);
        Assert.Equal(["a.png"], rebuilt.Outputs);
    }

    [Fact]
    public void Finished_jobs_older_than_a_week_are_pruned_on_write()
    {
        using var dir = new TempDir();
        var store = new JobStore(dir.Path);
        var old = JobSnapshot.From(new ConversionJob([dir.File("old.jpg")], new Preset { TargetFormat = "png" }, "PNG"));
        old.State = nameof(JobState.Completed);
        old.CreatedAt = DateTime.Now.AddDays(-8);
        var active = JobSnapshot.From(new ConversionJob([dir.File("new.jpg")], new Preset { TargetFormat = "png" }, "PNG"));

        store.Upsert(old);
        store.Upsert(active);

        var jobs = store.Load().Jobs;
        Assert.Single(jobs);
        Assert.Equal(active.Id, jobs[0].Id);
    }

    [Fact]
    public void A_broken_queue_file_is_kept_for_inspection_and_the_queue_starts_empty()
    {
        using var dir = new TempDir();
        var store = new JobStore(dir.Path);
        File.WriteAllText(store.FilePath, "{ not json");

        Assert.Empty(store.Load().Jobs);
        Assert.True(File.Exists(store.FilePath + ".broken"));
    }

    [Fact]
    public void A_disabled_store_reads_but_does_not_write()
    {
        using var dir = new TempDir();
        var store = new JobStore(dir.Path) { Enabled = false };
        store.Upsert(JobSnapshot.From(new ConversionJob([dir.File("a.jpg")], new Preset { TargetFormat = "png" }, "PNG")));

        Assert.False(File.Exists(store.FilePath));
        Assert.Empty(store.Load().Jobs);
    }
}
