// Portable v2 conversion workflow: a small ordered list of conversion steps, executed through the same JobQueue.

using Morfa.Domain.Conversion;
using Morfa.Domain.Formats;
using Morfa.Domain.Presets;
using Morfa.Domain.Settings;

namespace Morfa.Domain.Workflows;

/// <summary>A saved workflow. Format 2 is the first published format.</summary>
public sealed record RecipeFile
{
    public const int CurrentFormat = 2;

    public int Format { get; set; } = CurrentFormat;
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public List<RecipeStep> Steps { get; set; } = [];
}

/// <summary>One step. <c>op</c> is <c>convert</c> or <c>preset</c>; the other fields are its parameters.</summary>
public sealed record RecipeStep
{
    public string Op { get; set; } = "";

    /// <summary>Target format of a <c>convert</c> step.</summary>
    public string To { get; set; } = "";

    /// <summary>Id or display name of a saved preset for a <c>preset</c> step.</summary>
    public string Preset { get; set; } = "";

    /// <summary>Resize an image before the next convert step: long edge in pixels.</summary>
    public int MaxWidth { get; set; }

    /// <summary>Image quality 1 to 100 for the next convert step; 0 keeps the preset's value.</summary>
    public int Quality { get; set; }

    /// <summary>PDF text watermark for the next convert step.</summary>
    public string Watermark { get; set; } = "";
}

/// <summary>Validation result. Every error names the step that caused it.</summary>
public sealed record RecipeValidation(bool IsValid, IReadOnlyList<string> Errors)
{
    public static RecipeValidation Ok() => new(true, []);
    public static RecipeValidation Fail(string error) => new(false, [error]);
}

/// <summary>Validates and compiles recipes into presets that the queue can run.</summary>
public static class RecipeCompiler
{
    private static readonly string[] Operations = ["convert", "preset"];

    public static RecipeValidation Validate(RecipeFile recipe)
    {
        if (recipe.Format != RecipeFile.CurrentFormat)
            return RecipeValidation.Fail($"Unsupported recipe format {recipe.Format}; expected {RecipeFile.CurrentFormat}.");
        if (string.IsNullOrWhiteSpace(recipe.Id))
            return RecipeValidation.Fail("The recipe needs an id.");
        if (recipe.Steps is null || recipe.Steps.Count == 0)
            return RecipeValidation.Fail("The recipe needs at least one step.");
        for (var i = 0; i < recipe.Steps.Count; i++)
        {
            var step = recipe.Steps[i];
            if (step is null)
                return RecipeValidation.Fail($"Step {i}: empty step.");
            if (!Operations.Contains(step.Op, StringComparer.OrdinalIgnoreCase))
                return RecipeValidation.Fail($"Step {i}: unknown operation '{step.Op}'.");
            if (step.Op.Equals("convert", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(step.To))
                    return RecipeValidation.Fail($"Step {i}: convert needs a target format.");
                if (FormatRegistry.FindById(step.To.Trim()) is null)
                    return RecipeValidation.Fail($"Step {i}: unknown format '{step.To}'.");
            }
            if (step.Op.Equals("preset", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(step.Preset))
                return RecipeValidation.Fail($"Step {i}: preset needs a preset id or name.");
            if (step.MaxWidth < 0 || step.MaxWidth > 100_000)
                return RecipeValidation.Fail($"Step {i}: maxWidth must be 0 to 100000.");
            if (step.Quality is < 0 or > 100)
                return RecipeValidation.Fail($"Step {i}: quality must be 0 to 100.");
        }
        return RecipeValidation.Ok();
    }

    /// <summary>
    /// Builds one preset per step, carrying options such as resize and watermark into the step that follows them.
    /// A <c>preset</c> step starts from the named preset; a <c>convert</c> step from the previous step's preset.
    /// </summary>
    public static IReadOnlyList<Preset> Compile(RecipeFile recipe, Preset starter, Func<string, Preset?> findPreset)
    {
        var result = new List<Preset>();
        var current = Sanitize(starter.Clone());

        foreach (var step in recipe.Steps)
        {
            if (step.Op.Equals("preset", StringComparison.OrdinalIgnoreCase))
            {
                current = Sanitize(findPreset(step.Preset)?.Clone()
                    ?? throw new InvalidDataException($"Recipe '{recipe.Id}' uses unknown preset '{step.Preset}'."));
            }
            else
            {
                current = current.Clone();
                current.TargetFormat = step.To.Trim().ToLowerInvariant();
            }
            ApplyOptions(current, step);
            result.Add(current.Clone());
        }
        return result;
    }

    private static Preset Sanitize(Preset preset)
    {
        preset.Output.Location = OutputLocation.SameFolder;
        preset.Output.ReplaceOriginal = false;
        preset.Output.ZipResult = false;
        preset.Output.OnlyIfSmaller = false;
        preset.Pdf.MergeIntoSingle = false;
        preset.Archive.CombineIntoOne = false;
        preset.Output.KeepFoldersFrom = "";
        return preset;
    }

    private static void ApplyOptions(Preset preset, RecipeStep step)
    {
        if (step.MaxWidth > 0)
        {
            preset.Image.Resize = ResizeMode.LongEdge;
            preset.Image.LongEdge = step.MaxWidth;
        }
        if (step.Quality > 0)
            preset.Image.Quality = step.Quality;
        if (!string.IsNullOrWhiteSpace(step.Watermark))
            preset.Pdf.Watermark = step.Watermark;
    }
}
