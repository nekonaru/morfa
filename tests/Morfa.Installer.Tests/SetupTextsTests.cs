// Setup speaks Morfa's languages from the app's own translation files.

extern alias app;
using System.Text.Json;
using System.Text.RegularExpressions;
using Morfa.Converters.Infrastructure;
using Morfa.Installer.Platform;
using LocalizationService = app::Morfa.Desktop.Services.LocalizationService;

namespace Morfa.Installer.Tests;

public partial class SetupTextsTests
{
    private static string Repository
    {
        get
        {
            var dir = AppContext.BaseDirectory;
            while (!File.Exists(Path.Combine(dir, "Morfa.slnx")))
                dir = Path.GetDirectoryName(dir) ?? throw new InvalidOperationException("Repository root not found.");
            return dir;
        }
    }

    [Fact]
    public void Setup_offers_every_language_morfa_has()
    {
        Assert.Equal(LocalizationService.Languages.Select(l => (l.Code, l.NativeName)), SetupTexts.Languages);
        Assert.Contains(SetupTexts.Languages, l => l.Code == "en");
        Assert.Contains(("id", "Bahasa Indonesia"), SetupTexts.Languages);
        Assert.Equal("Pasang", new SetupTexts("id")["installer.install"]);
    }

    [Theory]
    [InlineData("id", "en-US", "id")]
    [InlineData("ID", "en-US", "id")]
    [InlineData("auto", "id-ID", "id")]
    [InlineData(null, "id-ID", "id")]
    [InlineData("auto", "in-ID", "id")] // the legacy Indonesian code
    [InlineData(null, "en-US", "en")]
    [InlineData("en-US", "id-ID", "id")] // Setup matches a shipped language by its primary subtag; English is the fallback
    [InlineData("xx", "id-ID", "id")]
    [InlineData("klingon", "en-US", "en")]
    [InlineData(null, "pt-BR", "en")]
    [InlineData(null, "zh-CN", "en")]
    [InlineData(null, "ko-KR", "en")]
    [InlineData(null, "ja-JP", "en")]
    [InlineData(null, "ar-EG", "en")]
    [InlineData("auto", "", "en")]
    public void Languages_resolve_like_the_app_and_like_inno_setup(string? setting, string windows, string expected) =>
        Assert.Equal(expected, SetupTexts.Resolve(setting, () => windows));

    [Fact]
    public void Texts_fall_back_to_english_and_format()
    {
        var texts = new SetupTexts("id");
        Assert.Equal("id", texts.Language);
        Assert.NotEqual("installer.install", texts["installer.install"]);
        Assert.Contains("9.9.9", texts.Format("installer.welcome.fresh", "9.9.9"));
        Assert.Equal("no.such.key", texts["no.such.key"]);
        var changed = 0;
        texts.LanguageChanged += (_, _) => changed++;
        texts.SetLanguage("en");
        Assert.Equal(1, changed);
        Assert.Equal("Install", texts["installer.install"]);
    }

    [Fact]
    public void Every_key_setup_uses_exists_in_english()
    {
        var english = JsonSerializer.Deserialize<Dictionary<string, string>>(
            File.ReadAllText(Path.Combine(Repository, "src", "Morfa.Desktop", "Assets", "i18n", "en.json")))!;
        var source = Path.Combine(Repository, "src", "Morfa.Installer");
        var used = Directory.EnumerateFiles(source, "*.axaml", SearchOption.AllDirectories)
            .SelectMany(f => DynamicResource().Matches(File.ReadAllText(f)).Select(m => m.Groups[1].Value))
            .Concat(Directory.EnumerateFiles(source, "*.cs", SearchOption.AllDirectories)
                .SelectMany(f => KeyLiteral().Matches(File.ReadAllText(f)).Select(m => m.Groups[1].Value)))
            .Where(k => k.Contains('.') && !k.EndsWith('.') && !k.StartsWith("Setup.", StringComparison.Ordinal))
            .Distinct()
            .ToList();
        Assert.Contains("installer.welcome.fresh", used);
        Assert.Contains("general.context_menu", used);
        Assert.DoesNotContain(used, k => !english.ContainsKey(k));
        // Engine names come from the app's own keys.
        Assert.All(EngineDownloads.Packages, p => Assert.True(english.ContainsKey($"engines.package.{p.Id}.name"), p.Id));
    }

    [Fact]
    public void Setup_looks_for_downloaded_engines_where_the_app_puts_them()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "The installer uses Windows settings and engine paths.");
        Assert.Equal(EngineEnvironment.DownloadRoot, SetupPaths.Real().EnginesFolder);
        Assert.Equal(Morfa.Domain.Settings.UserDataStore.DefaultDirectory, SetupPaths.Real().SettingsFolder);
    }

    [GeneratedRegex(@"\{DynamicResource ([\w.]+)\}")]
    private static partial Regex DynamicResource();

    [GeneratedRegex(@"""((?:installer|setup|engines|general|common)\.[\w.]+)""")]
    private static partial Regex KeyLiteral();
}
