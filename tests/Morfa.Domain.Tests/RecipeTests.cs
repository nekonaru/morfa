// Tests for saved v2 workflows: validation, round trip and ordered execution.

using System.Text.Json;
using Morfa.Domain.Conversion;
using Morfa.Domain.Presets;
using Morfa.Domain.Settings;
using Morfa.Domain.Workflows;

namespace Morfa.Domain.Tests;

public class RecipeTests
{
    [Fact]
    public async Task Two_convert_steps_run_in_order_and_use_the_previous_output()
    {
        using var dir = new TempDir();
        var source = dir.File("photo.jpg");
        var catalog = new ConverterCatalog([
            new FakeConverter("magick", new ConversionEdge("jpg", "png"), new ConversionEdge("png", "webp")),
        ]);
        await using var queue = new JobQueue(catalog);
        var recipe = new RecipeFile
        {
            Id = "two-steps",
            Name = "Two steps",
            Steps =
            [
                new RecipeStep { Op = "convert", To = "png" },
                new RecipeStep { Op = "convert", To = "webp" },
            ],
        };
        var validation = RecipeCompiler.Validate(recipe);
        Assert.True(validation.IsValid, string.Join(", ", validation.Errors));

        var presets = RecipeCompiler.Compile(recipe, new Preset { TargetFormat = "png" }, _ => null);
        var outputs = await new RecipeRunner(queue).RunAsync(recipe, [source], presets, dir.Path,
            TestContext.Current.CancellationToken);

        var output = Assert.Single(outputs);
        Assert.EndsWith(".webp", output);
        Assert.Contains("photo.png", File.ReadAllText(output));
        Assert.Contains("webp by magick", File.ReadAllText(output));
        Assert.False(File.Exists(Path.Combine(dir.Path, "photo.png")), "intermediates must not stay next to the source");
    }

    [Fact]
    public void An_unknown_operation_names_the_step_and_the_operation()
    {
        var recipe = new RecipeFile
        {
            Id = "bad",
            Steps = [new RecipeStep { Op = "convert", To = "png" }, new RecipeStep { Op = "watermark" }],
        };

        var validation = RecipeCompiler.Validate(recipe);

        Assert.False(validation.IsValid);
        Assert.Contains("Step 1", validation.Errors[0]);
        Assert.Contains("watermark", validation.Errors[0]);
    }

    [Fact]
    public void A_version_1_recipe_is_refused()
    {
        var recipe = new RecipeFile { Id = "old", Format = 1, Steps = [new RecipeStep { Op = "convert", To = "png" }] };

        var validation = RecipeCompiler.Validate(recipe);

        Assert.False(validation.IsValid);
        Assert.Contains("format 1", validation.Errors[0]);
        Assert.Contains("expected 2", validation.Errors[0]);
    }

    [Fact]
    public void Recipe_json_round_trips_identically()
    {
        var recipe = new RecipeFile
        {
            Id = "round-trip",
            Name = "Round trip",
            Steps =
            [
                new RecipeStep { Op = "convert", To = "png", MaxWidth = 1920, Quality = 80 },
                new RecipeStep { Op = "preset", Preset = "to-pdf" },
            ],
        };

        var json = JsonSerializer.Serialize(recipe, MorfaJsonContext.Default.RecipeFile);
        var loaded = JsonSerializer.Deserialize(json, MorfaJsonContext.Default.RecipeFile)!;
        var again = JsonSerializer.Serialize(loaded, MorfaJsonContext.Default.RecipeFile);

        Assert.Equal(json, again);
        Assert.Equal(2, loaded.Steps.Count);
    }
}
