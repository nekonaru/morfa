using Avalonia;
using Avalonia.Headless;
using Morfa.Desktop;
using Morfa.Desktop.Services;
using Morfa.Domain.Settings;

[assembly: AvaloniaTestApplication(typeof(Morfa.Desktop.Tests.TestAppBuilder))]
// One application for all tests. Per test, Avalonia disposes the font manager and loads the embedded Noto fonts
// again, and on Windows the test process then crashed with 0xC0000005 (only builds with the fonts, i.e. releases).
[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerAssembly)]

namespace Morfa.Desktop.Tests;

/// <summary>Headless Avalonia with real Skia rendering so views can be captured as images.</summary>
public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UseSkia()
            .UseHarfBuzz()
            .ConfigureFonts(fonts => fonts.AddFontCollection(new MorfaFontCollection()))
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}

/// <summary>Builds the service container once against a throw-away data folder (never touches %APPDATA%).</summary>
internal static class TestServices
{
    private static Application? _initializedFor;

    public static string DataDirectory { get; } =
        Path.Combine(Path.GetTempPath(), "morfa-app-tests", Guid.NewGuid().ToString("N")[..8]);

    /// <summary>
    /// The headless runner may create a new Application per test, so services (and the localization
    /// dictionary attached to Application.Resources) are rebuilt whenever the application changes.
    /// </summary>
    public static void EnsureInitialized(string language = "en")
    {
        var app = Application.Current!;
        if (!ReferenceEquals(_initializedFor, app))
        {
            AppHost.Build(DataDirectory);
            AppHost.Get<UserDataStore>().Load();
            AppHost.Get<LocalizationService>().Attach(app);
            _initializedFor = app;
        }

        // The UI language sets CurrentUICulture; CurrentCulture stays the user's regional setting. Pin it so number,
        // date and file-size formatting is deterministic across tests (the host's own setting otherwise leaks in).
        System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");
        var store = AppHost.Get<UserDataStore>();
        store.Settings.Language = language;
        AppHost.Get<LocalizationService>().SetLanguage(language);
        System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");
        AppHost.Get<ThemeService>().Apply(store.Settings.Theme, language);
    }

    /// <summary>Where rendered screenshots go: tests/Morfa.Desktop.Tests/bin/.../screenshots.</summary>
    public static string ScreenshotDirectory
    {
        get
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "screenshots");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }
}
