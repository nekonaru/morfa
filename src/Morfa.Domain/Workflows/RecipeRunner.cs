// Runs a compiled recipe through the same queue as a normal conversion. Intermediate files stay in a temp folder.

using Morfa.Domain.Conversion;
using Morfa.Domain.Presets;

namespace Morfa.Domain.Workflows;

public sealed class RecipeRunner(JobQueue queue)
{
    /// <summary>Runs every step in order and returns the final outputs. The work folder is deleted afterwards.</summary>
    public async Task<IReadOnlyList<string>> RunAsync(RecipeFile recipe, IReadOnlyList<string> sources,
        IReadOnlyList<Preset> presets, string? outputFolder = null, CancellationToken cancellationToken = default)
    {
        if (presets.Count == 0)
            throw new InvalidDataException($"Recipe '{recipe.Id}' has no steps.");
        if (sources.Count == 0)
            throw new ArgumentException("A recipe needs at least one source file.", nameof(sources));
        var destination = outputFolder ?? Path.GetDirectoryName(Path.GetFullPath(sources[0]))!;
        Directory.CreateDirectory(destination);
        var root = Path.Combine(Path.GetTempPath(), "Morfa", "recipes", Guid.NewGuid().ToString("N")[..10]);
        Directory.CreateDirectory(root);
        try
        {
            var current = sources;
            for (var i = 0; i < presets.Count; i++)
            {
                var preset = presets[i].Clone();
                if (i < presets.Count - 1)
                {
                    var folder = Path.Combine(root, $"step{i}");
                    Directory.CreateDirectory(folder);
                    preset.Output.Location = OutputLocation.CustomFolder;
                    preset.Output.CustomFolder = folder;
                    preset.Output.FileNamePattern = "{name}";
                    preset.Output.Conflict = ConflictPolicy.Overwrite;
                }
                else
                {
                    // The last step must not write into a temp folder that is about to be deleted ("next to the
                    // source" would do exactly that, because the source is now an intermediate).
                    preset.Output.Location = OutputLocation.CustomFolder;
                    preset.Output.CustomFolder = destination;
                }
                var job = new ConversionJob(current, preset, recipe.Name);
                using var registration = cancellationToken.Register(job.Cancel);
                await queue.RunAsync(job);
                if (job.State is JobState.Failed or JobState.Cancelled)
                    throw new InvalidOperationException($"Recipe '{recipe.Id}' step {i} ended as {job.State}.");
                current = job.Files.SelectMany(f => f.Outputs).ToList();
                if (current.Count == 0)
                    throw new InvalidOperationException($"Recipe '{recipe.Id}' step {i} produced no output.");
            }
            return current;
        }
        finally
        {
            try { Directory.Delete(root, recursive: true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
