using System.Reflection;

namespace Morfa.Installer;

/// <summary>Facts about this Setup program.</summary>
public static class SetupInfo
{
    /// <summary>GitHub repository from Directory.Build.props (MorfaRepositoryUrl).</summary>
    public static string RepositoryUrl =>
        typeof(SetupInfo).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "RepositoryUrl")?.Value ?? "https://github.com/nekonaru/morfa";

    /// <summary>The download page: one big download button.</summary>
    public const string WebsiteUrl = "https://github.com/nekonaru/morfa/releases/latest";

    /// <summary>The version this Setup program was built as (the payload says which Morfa it installs).</summary>
    public static string Version =>
        typeof(SetupInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "0.0.0";
}
