using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Morfa.Platform.MacOS;

namespace Morfa.Platform.Tests;

public sealed class MacOSIntegrationTests
{
    [Fact]
    public void LaunchAgentKeepsExecutableAndArgumentsSeparate()
    {
        const string executable = "/Users/a & b/도구 '$(touch nope)'/Morfa.app/Contents/MacOS/Morfa";
        var root = ReadPlist(MacOSIntegrationFiles.LaunchAgent(executable));
        Assert.Equal([executable, "--background"], Property(root, "ProgramArguments").Elements().Select(x => x.Value));
        Assert.Equal("true", Property(root, "RunAtLoad").Name.LocalName);
        Assert.Equal("false", Property(root, "KeepAlive").Name.LocalName);
        Assert.Equal("Aqua", Property(root, "LimitLoadToSessionType").Value);
    }

    [Fact]
    public void ServiceSwitchWritesTheEntrySystemSettingsUses()
    {
        Assert.Equal("\"com.morfa.app - Convert with Morfa - convertFiles\"", MacOSIntegrationFiles.ServiceStatusKey);
        Assert.Contains("\"enabled_context_menu\" = 0;", MacOSIntegrationFiles.ServiceStatus(false));
        Assert.Contains("\"ContextMenu\" = 1;", MacOSIntegrationFiles.ServiceStatus(true));
    }

    [Fact]
    public void AppBundleDeclaresTheServiceMorfaAnswers()
    {
        var installer = Path.Combine(RepositoryRoot(), "installer", "macos");
        var root = ReadPlist(File.ReadAllText(Path.Combine(installer, "Info.plist")));
        var service = Property(root, "NSServices").Element("dict")!;
        Assert.Equal(MacOSServicesProvider.MenuItem, Property(Property(service, "NSMenuItem"), "default").Value);
        Assert.Equal(MacOSServicesProvider.Message, Property(service, "NSMessage").Value);
        Assert.Equal("Morfa", Property(service, "NSPortName").Value);
        Assert.Equal("public.item", Property(service, "NSSendFileTypes").Element("string")!.Value);
        // Each translation names the same menu title.
        foreach (var strings in Directory.GetFiles(installer, "ServicesMenu.strings", SearchOption.AllDirectories))
            Assert.StartsWith($"\"{MacOSServicesProvider.MenuItem}\" = ", File.ReadAllLines(strings).Single(l => l.StartsWith('"')));
        var declared = Property(root, "CFBundleLocalizations").Elements("string").Select(x => x.Value).Order();
        var folders = Directory.GetFiles(installer, "ServicesMenu.strings", SearchOption.AllDirectories)
            .Select(path => Path.GetFileNameWithoutExtension(Path.GetDirectoryName(path)!)).Order();
        Assert.Equal(declared, folders);
    }

    [Fact]
    public void NativeTextsMatchTheAppAndKeepThePermissionWording()
    {
        const string language = "id";
        var repository = RepositoryRoot();
        var installer = Path.Combine(repository, "installer", "macos");
        var root = ReadPlist(File.ReadAllText(Path.Combine(installer, "Info.plist")));
        Assert.Contains(language, Property(root, "CFBundleLocalizations").Elements("string").Select(x => x.Value));
        var folder = Path.Combine(installer, language + ".lproj");
        var menu = ReadStrings(Path.Combine(folder, "ServicesMenu.strings"));
        using var app = JsonDocument.Parse(File.ReadAllText(Path.Combine(repository, "src", "Morfa.App", "Assets", "i18n", language + ".json")));
        Assert.Equal(app.RootElement.GetProperty("general.context_menu_label").GetString(), menu[MacOSServicesProvider.MenuItem]);

        var permissions = ReadStrings(Path.Combine(folder, "InfoPlist.strings"));
        Assert.Equal(2, permissions.Count);
        foreach (var (key, value) in permissions)
        {
            Assert.NotEqual(Property(root, key).Value, value);
            Assert.StartsWith("Morfa ", value);
            Assert.True(value.IsNormalized(), $"{language}: {key} is not NFC");
        }
        Assert.Contains("Finder", permissions["NSAppleEventsUsageDescription"]);
        Assert.Contains("gestur", permissions["NSInputMonitoringUsageDescription"]);
    }

    private static Dictionary<string, string> ReadStrings(string path) =>
        Regex.Matches(File.ReadAllText(path).ReplaceLineEndings("\n"), "^\"([^\"]+)\" = \"([^\"]+)\";$", RegexOptions.Multiline)
            .ToDictionary(match => match.Groups[1].Value, match => match.Groups[2].Value);

    private static string RepositoryRoot()
    {
        var folder = new DirectoryInfo(AppContext.BaseDirectory);
        while (folder is not null && !File.Exists(Path.Combine(folder.FullName, "Morfa.slnx")))
            folder = folder.Parent;
        return folder?.FullName ?? throw new DirectoryNotFoundException("Morfa.slnx not found above the test output.");
    }

    [Fact]
    public void SelectionJsonPreservesWhitespaceUnicodeAndNewlines()
    {
#pragma warning disable CA1416 // Pure JSON parsing does not access the operating system.
        var paths = MacOSPlatformServices.ParseSelection("[\"/tmp/ 한글 .txt\",\"/tmp/line\\nbreak.txt\",\"/tmp/quote\\\".txt\"]");
#pragma warning restore CA1416
        Assert.Equal(["/tmp/ 한글 .txt", "/tmp/line\nbreak.txt", "/tmp/quote\".txt"], paths);
    }

    private static XElement ReadPlist(string text)
    {
        var document = XDocument.Parse(text);
        Assert.Equal("plist", document.Root!.Name.LocalName);
        Assert.Equal("1.0", document.Root.Attribute("version")!.Value);
        return document.Root.Element("dict")!;
    }

    private static XElement Property(XElement dictionary, string key) =>
        dictionary.Elements("key").Single(x => x.Value == key).ElementsAfterSelf().First();
}
