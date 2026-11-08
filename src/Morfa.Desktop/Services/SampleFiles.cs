// The built-in sample picture a new user can try converting without having a file of their own.

using Avalonia.Platform;

namespace Morfa.Desktop.Services;

/// <summary>The first-run sample: a photo-like picture bundled with the app, so the first conversion needs no file.</summary>
internal static class SampleFiles
{
    internal const string Uri = "avares://Morfa/Assets/Sample/sample.png";
    internal const string FileName = "morfa-sample.png";

    /// <summary>Copies the sample into a temp file and returns its path, or null when it cannot be read.</summary>
    public static string? Copy()
    {
        var uri = new Uri(Uri, UriKind.Absolute);
        if (!AssetLoader.Exists(uri))
            return null;
        var path = Path.Combine(Path.GetTempPath(), FileName);
        try
        {
            using var input = AssetLoader.Open(uri);
            using var output = File.Create(path);
            input.CopyTo(output);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }
        return path;
    }
}
