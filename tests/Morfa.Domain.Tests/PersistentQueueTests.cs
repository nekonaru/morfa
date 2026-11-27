// Tests for the queue's persistence seam: jobs must survive a restart without losing their state.

using Morfa.Domain.Conversion;
using Morfa.Domain.Presets;
using Morfa.Domain.Queue;

namespace Morfa.Domain.Tests;

public class PersistentQueueTests
{
    [Fact]
    public async Task Enqueuing_saves_the_job_and_finishing_saves_the_final_state()
    {
        using var dir = new TempDir();
        var store = new JobStore(dir.Path);
        await using var queue = new JobQueue(
            new ConverterCatalog([new FakeConverter("magick", new ConversionEdge("jpg", "png"))]), store: store);

        var job = queue.Enqueue([dir.File("a.jpg")], new Preset { TargetFormat = "png" }, "PNG");

        var saved = Assert.Single(store.Load().Jobs);
        Assert.Equal(job.Id, saved.Id);
        Assert.Contains(saved.State, Enum.GetNames<JobState>()); // the worker may already have picked it up
        Assert.True(await WaitFor(() => store.Load().Jobs[0].State == nameof(JobState.Completed)));
    }

    [Fact]
    public async Task A_job_cut_short_by_a_restart_is_queued_and_run_again()
    {
        using var dir = new TempDir();
        var store = new JobStore(dir.Path);
        var snapshot = JobSnapshot.From(new ConversionJob([dir.File("a.jpg")],
            new Preset { TargetFormat = "png" }, "PNG"));
        snapshot.State = nameof(JobState.Running);
        snapshot.Files[0].State = nameof(FileState.Running);
        store.Upsert(snapshot);

        await using var restarted = new JobQueue(
            new ConverterCatalog([new FakeConverter("magick", new ConversionEdge("jpg", "png"))]),
            store: store, restoreOnStartup: true);

        Assert.True(await WaitFor(() => store.Load().Jobs[0].State == nameof(JobState.Completed)));
        var finished = store.Load().Jobs[0];
        Assert.Equal(snapshot.Id, finished.Id);
        Assert.Equal(nameof(FileState.Done), finished.Files[0].State);
        Assert.Single(finished.Files[0].Outputs);
    }

    [Fact]
    public async Task A_paused_job_is_not_run_after_a_restart()
    {
        using var dir = new TempDir();
        var store = new JobStore(dir.Path);
        var snapshot = JobSnapshot.From(new ConversionJob([dir.File("a.jpg")],
            new Preset { TargetFormat = "png" }, "PNG"));
        snapshot.Paused = true;
        store.Upsert(snapshot);

        await using var restarted = new JobQueue(
            new ConverterCatalog([new FakeConverter("magick", new ConversionEdge("jpg", "png"))]),
            store: store, restoreOnStartup: true);

        await Task.Delay(500, TestContext.Current.CancellationToken);
        Assert.Equal(nameof(JobState.Queued), store.Load().Jobs[0].State);
        Assert.False(restarted.IsBusy);
    }

    private static async Task<bool> WaitFor(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
                return true;
            await Task.Delay(50, TestContext.Current.CancellationToken);
        }
        return false;
    }
}
