// The conversion pipeline without the app: the user's settings and presets, every engine (bundled ones next to the
// executable, downloaded ones in the per-user engines folder) and a job queue. Mirrors AppHost in Morfa.Desktop.

using Morfa.Converters;
using Morfa.Converters.Infrastructure;
using Morfa.Domain.Conversion;
using Morfa.Domain.Localization;
using Morfa.Domain.Platform;
using Morfa.Domain.Queue;
using Morfa.Domain.Settings;

namespace Morfa.CommandLine;

internal sealed class CliHost : IAsyncDisposable
{
    private CliHost(UserDataStore store, ConverterCatalog catalog, JobQueue queue, JobStore queueStore)
    {
        Store = store;
        Catalog = catalog;
        Queue = queue;
        QueueStore = queueStore;
    }

    public UserDataStore Store { get; }
    public ConverterCatalog Catalog { get; }
    public JobQueue Queue { get; }
    public JobStore QueueStore { get; }
    public ILocalizer Texts { get; } = new EnglishTexts();

    /// <param name="dataDirectory">Settings folder; the app's (%APPDATA%\Morfa) when null.</param>
    public static CliHost Create(string? dataDirectory = null)
    {
        var store = new UserDataStore(dataDirectory ?? UserDataStore.DefaultDirectory);
        store.Load();
        EngineEnvironment.Offline = store.Settings.Engine?.Offline == true;
        EngineEnvironment.OwnCopies = new Dictionary<string, string>(store.Settings.EngineOwnCopies);
        EngineEnvironment.LibreOfficeForSlides = store.Settings.LibreOfficeForSlides;
        EngineEnvironment.LibreOfficeForHwp = store.Settings.LibreOfficeForHwp;
        EngineEnvironment.LibreOfficeForDocuments = store.Settings.LibreOfficeForDocuments;
        EngineEnvironment.LibreOfficeForSheets = store.Settings.LibreOfficeForSheets;
        var converters = EngineRegistry.CreateAll(new EngineEnvironment(store.Directory));
        var catalog = new ConverterCatalog(converters) { Priority = store.Settings.EnginePriority };
        var queueStore = new JobStore(store.Directory) { Enabled = store.Settings.Queue.Persist };
        var queue = new JobQueue(catalog, EngineRegistry.FindPdfMerger(converters), combiner: EngineRegistry.FindFileCombiner(converters),
            tiffMerger: EngineRegistry.FindTiffMerger(converters), mediaJoiner: EngineRegistry.FindMediaJoiner(converters),
            moveToTrash: Trash(), store: queueStore);
        return new CliHost(store, catalog, queue, queueStore);
    }

    /// <summary>The system's Recycle Bin / Trash for "Replace the original", as in the app; none elsewhere.</summary>
    private static Func<string, TrashRefusal?>? Trash() =>
        OperatingSystem.IsWindows() ? new Morfa.Platforms.Windows.WindowsPlatformServices().MoveToTrash
        : OperatingSystem.IsMacOS() ? new Morfa.Platforms.MacOS.MacOSPlatformServices().MoveToTrash
        : OperatingSystem.IsLinux() ? new Morfa.Platforms.Linux.LinuxPlatformServices().MoveToTrash
        : null;

    public ValueTask DisposeAsync() => Queue.DisposeAsync();
}
