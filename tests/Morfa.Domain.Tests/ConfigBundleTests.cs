// F4 tests: configuration bundle round trip and newer-format refusal.

using Morfa.Domain.Presets;
using Morfa.Domain.Settings;

namespace Morfa.Domain.Tests;

public class ConfigBundleTests
{
    [Fact]
    public void Config_bundle_round_trips_settings_presets_and_profiles()
    {
        using var dir = new TempDir();
        var store = new UserDataStore(dir.Path);
        store.Load();
        store.Settings.Language = "id";
        store.Settings.Engine.Offline = true;
        store.Settings.Engine.PinnedVersions["ffmpeg"] = "7.0";
        store.Presets.Add(new Preset { Id = "my-preset", TargetFormat = "webp" });
        store.SaveSettings();
        store.SaveLibrary();
        var bundle = Path.Combine(dir.Path, "config.json");

        store.ExportConfig(bundle);
        var restored = new UserDataStore(Path.Combine(dir.Path, "restored"));
        restored.ImportConfig(bundle);

        Assert.Equal("id", restored.Settings.Language);
        Assert.True(restored.Settings.Engine.Offline);
        Assert.Equal("7.0", restored.Settings.Engine.PinnedVersions["ffmpeg"]);
        Assert.Contains(restored.Presets, p => p.Id == "my-preset");
        Assert.NotEmpty(restored.Profiles);
    }

    [Fact]
    public void A_bundle_from_a_newer_version_is_refused()
    {
        using var dir = new TempDir();
        var store = new UserDataStore(dir.Path);
        store.Load();
        var bundle = Path.Combine(dir.Path, "config.json");
        store.ExportConfig(bundle);
        var node = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(bundle))!.AsObject();
        node["schemaVersion"] = 99;
        File.WriteAllText(bundle, node.ToJsonString());

        var restored = new UserDataStore(Path.Combine(dir.Path, "restored"));

        var error = Assert.Throws<InvalidDataException>(() => restored.ImportConfig(bundle));
        Assert.Contains("newer version", error.Message);
    }
}
