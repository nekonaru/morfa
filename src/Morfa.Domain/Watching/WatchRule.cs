// A watch folder: files that land in the folder are converted with a preset (Settings → Watch folders, or
// "morfa watch" on the command line).

using Morfa.Domain.Presets;

namespace Morfa.Domain.Watching;

/// <summary>What happens to a source file once it was converted.</summary>
public enum AfterConversion
{
    /// <summary>The source stays where it is.</summary>
    Keep,

    /// <summary>The source moves into <see cref="WatchRule.OriginalsFolder"/>, so the watch folder works like an inbox.</summary>
    MoveToOriginals,
}

/// <summary>One watched folder and how its files are converted.</summary>
public sealed class WatchRule
{
    /// <summary>Name of the default output folder inside the watched folder.</summary>
    public const string DefaultOutputName = "converted";

    /// <summary>Name of the folder inside the watched folder that receives converted sources (<see cref="AfterConversion.MoveToOriginals"/>).</summary>
    public const string OriginalsName = "originals";

    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    public bool Enabled { get; set; } = true;

    public string Folder { get; set; } = "";

    /// <summary>Preset to convert with (<see cref="Presets.Preset.Id"/>).</summary>
    public string PresetId { get; set; } = "";

    public bool IncludeSubfolders { get; set; }

    /// <summary>Where converted files go; empty means <c>converted</c> inside <see cref="Folder"/>.</summary>
    public string OutputFolder { get; set; } = "";

    public AfterConversion Originals { get; set; } = AfterConversion.Keep;

    // Optional: empty means every file, and names as the preset gives them.

    /// <summary>Which files are converted (<see cref="FileNameFilter"/>), e.g. <c>*.heic</c> and <c>scan_*</c>; empty for all.</summary>
    public List<string> Include { get; set; } = [];

    /// <summary>File name pattern of converted files (tokens as in <see cref="OutputRule.FileNamePattern"/>); empty for the preset's.</summary>
    public string FileNamePattern { get; set; } = "";

    /// <summary>Replacements applied in order to the names of converted files (<see cref="OutputRule.Renames"/>).</summary>
    public List<RenameStep> Renames { get; set; } = [];

    /// <summary>
    /// Conditional routes, checked in order. When any are set, the first matching route's preset is used instead of
    /// <see cref="PresetId"/>; a file that matches none is skipped. An empty list keeps the single-preset behaviour.
    /// </summary>
    public List<WatchRoute> Routes { get; set; } = [];

    /// <summary>Delete outputs older than this many days; 0 keeps them forever.</summary>
    public int RetentionDays { get; set; }

    /// <summary>The preset id chosen for <paramref name="path"/>, or null when no route and no default applies.</summary>
    public string? PresetIdFor(string path)
    {
        if (Routes.Count == 0)
            return PresetId;
        var name = Path.GetFileName(path);
        var extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
        long bytes = -1;
        foreach (var route in Routes)
        {
            if (route.NamePatterns.Count > 0 && !route.NamePatterns.Any(p => FileNameFilter.Of([p]).Matches(name)))
                continue;
            if (route.Formats.Count > 0 && !route.Formats.Any(f =>
                    f.Trim().TrimStart('.').Equals(extension, StringComparison.OrdinalIgnoreCase)))
                continue;
            if (route.MaxSizeMb > 0 || route.MinSizeMb > 0)
            {
                if (bytes < 0)
                {
                    try { bytes = new FileInfo(path).Length; }
                    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { bytes = 0; }
                }
                if (route.MinSizeMb > 0 && bytes < route.MinSizeMb * 1024L * 1024) continue;
                if (route.MaxSizeMb > 0 && bytes > route.MaxSizeMb * 1024L * 1024) continue;
            }
            return route.PresetId;
        }
        return null;
    }

    /// <summary>The output folder actually used.</summary>
    public string ResolvedOutputFolder =>
        string.IsNullOrWhiteSpace(OutputFolder) ? Path.Combine(Folder, DefaultOutputName) : OutputFolder;

    /// <summary>Where converted sources go with <see cref="AfterConversion.MoveToOriginals"/>.</summary>
    public string OriginalsFolder => Path.Combine(Folder, OriginalsName);

    /// <summary>
    /// The preset as this folder converts with it: output into <see cref="ResolvedOutputFolder"/>, files from
    /// subfolders into the same subfolders there (as <see cref="AfterConversion.MoveToOriginals"/> does with the
    /// sources), and the folder's own name rules where set.
    /// </summary>
    public Preset Apply(Preset preset)
    {
        var applied = preset.Clone();
        applied.Output.Location = OutputLocation.CustomFolder;
        applied.Output.CustomFolder = ResolvedOutputFolder;
        applied.Output.KeepFoldersFrom = Folder;
        // A watch folder never replaces what it watches: the result would be converted again.
        applied.Output.ReplaceOriginal = false;
        if (!string.IsNullOrWhiteSpace(FileNamePattern))
            applied.Output.FileNamePattern = FileNamePattern.Trim();
        // Steps without a pattern are being typed; leaving them out keeps the name as it is.
        applied.Output.Renames.AddRange(Renames.Where(r => !r.IsBlank).Select(r => r.Clone()));
        return applied;
    }

    public WatchRule Clone()
    {
        var clone = (WatchRule)MemberwiseClone();
        clone.Include = [.. Include];
        clone.Renames = Renames.Select(r => r.Clone()).ToList();
        clone.Routes = Routes.Select(r => r.Clone()).ToList();
        return clone;
    }
}

/// <summary>One conditional route of a watch folder.</summary>
public sealed class WatchRoute
{
    public string PresetId { get; set; } = "";

    /// <summary>File name patterns, any match is enough (<see cref="FileNameFilter"/>).</summary>
    public List<string> NamePatterns { get; set; } = [];

    /// <summary>Source format ids or extensions, any match is enough.</summary>
    public List<string> Formats { get; set; } = [];

    /// <summary>Size bounds in megabytes; 0 means no bound.</summary>
    public double MinSizeMb { get; set; }
    public double MaxSizeMb { get; set; }

    public WatchRoute Clone()
    {
        var clone = (WatchRoute)MemberwiseClone();
        clone.NamePatterns = [.. NamePatterns];
        clone.Formats = [.. Formats];
        return clone;
    }
}
