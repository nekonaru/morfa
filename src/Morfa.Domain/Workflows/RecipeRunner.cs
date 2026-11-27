// Runs a compiled recipe through the same queue as a normal conversion. Intermediate files stay in a temp folder.

using Morfa.Domain.Conversion;
using Morfa.Domain.Presets;

namespace Morfa.Domain.Workflows;

/// <summary>The outcome of a recipe run: final outputs plus the files that failed along the way.</summary>
public sealed record RecipeRunResult(IReadOnlyList<string> Outputs, int Failed, IReadOnlyList<string> Errors);

public sealed class RecipeRunner(JobQueue queue)
{
    /// <summary>Runs every step in order and returns stable final paths. The work folder is deleted afterwards.</summary>
    public async Task<RecipeRunResult> RunAsync(RecipeFile recipe, IReadOnlyList<string> sources,
        IReadOnlyList<Preset> presets, string? outputFolder = null, CancellationToken cancellationToken = default)
    {
        if (presets.Count == 0)
            throw new InvalidDataException($"Recipe '{recipe.Id}' has no steps.");
        if (sources.Count == 0)
            throw new ArgumentException("A recipe needs at least one source file.", nameof(sources));

        // A job-id-shaped folder under the work root is also picked up by the crash sweeper.
        var root = Path.Combine(JobQueue.DefaultWorkRoot, Guid.NewGuid().ToString("N")[..10]);
        Directory.CreateDirectory(root);
        var errors = new List<string>();
        var failed = 0;
        try
        {
            var current = sources.ToList();
            // Where each live file originally came from, so a default final step writes next to its own source.
            var origins = sources.Select(s => Path.GetDirectoryName(Path.GetFullPath(s))!).ToList();
            for (var i = 0; i < presets.Count; i++)
            {
                var preset = presets[i].Clone();
                var last = i == presets.Count - 1;
                if (!last || outputFolder is not null)
                {
                    var folder = !last
                        ? Path.Combine(root, $"step{i}")
                        : Path.GetFullPath(outputFolder!);
                    Directory.CreateDirectory(folder);
                    preset.Output.Location = OutputLocation.CustomFolder;
                    preset.Output.CustomFolder = folder;
                }
                var job = new ConversionJob(current, preset, recipe.Name);
                using var registration = cancellationToken.Register(job.Cancel);
                await queue.RunAsync(job);

                var next = new List<string>();
                var nextOrigins = new List<string>();
                for (var f = 0; f < job.Files.Count && f < origins.Count; f++)
                {
                    var file = job.Files[f];
                    if (file.State is FileState.Failed or FileState.Cancelled)
                        failed++;
                    if (file.ErrorKey is not null)
                        errors.Add($"{file.SourcePath}: {file.ErrorKey}");
                    foreach (var output in file.Outputs)
                    {
                        if (last && outputFolder is null)
                        {
                            // The source is now an intermediate in a temp folder. Move the result back beside the
                            // original file so it survives the work folder being deleted.
                            var destination = PlaceNextTo(origins[f], output);
                            next.Add(destination);
                        }
                        else
                        {
                            next.Add(output);
                        }
                        nextOrigins.Add(origins[f]);
                    }
                }
                current = next;
                origins = nextOrigins;
                if (current.Count == 0)
                    throw new InvalidOperationException($"Recipe '{recipe.Id}' step {i} produced no output.");
            }
            return new RecipeRunResult(current, failed, errors);
        }
        finally
        {
            try { Directory.Delete(root, recursive: true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    /// <summary>Moves an output beside the original source, numbering it when that name is taken.</summary>
    private static string PlaceNextTo(string folder, string output)
    {
        Directory.CreateDirectory(folder);
        var target = Path.Combine(folder, Path.GetFileName(output));
        for (var n = 2; File.Exists(target); n++)
        {
            var name = Path.GetFileNameWithoutExtension(output);
            target = Path.Combine(folder, $"{name} ({n}){Path.GetExtension(output)}");
        }
        File.Move(output, target);
        return target;
    }
}
