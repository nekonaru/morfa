// Drop zone: files dropped on the Home page open the donut (click mode) at the drop position, also pictures, links and
// downloads dragged from a browser. A double-click opens the file picker, and the donut opens for the picked files.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.Logging;
using Morfa.Desktop.Services;
using Morfa.Domain.Localization;
using Morfa.Domain.Settings;

namespace Morfa.Desktop.Views.Pages;

public partial class HomePage : UserControl
{
    /// <summary>
    /// The first drag over the drop zone of this run is logged: a log without it after "drops don't work" means the
    /// drag never reached Morfa (Windows refuses drops from Explorer into a Morfa running as administrator).
    /// </summary>
    private static int _dragEnterLogged;

    public HomePage()
    {
        InitializeComponent();
        DropZone.DoubleTapped += OnDropZoneDoubleTapped;
        DragDrop.AddDragEnterHandler(DropZone, (_, e) =>
        {
            DropZone.Classes.Add("over");
            var files = DraggedFiles.Offered(e.DataTransfer);
            e.DragEffects = files ? DragDropEffects.Copy : DragDropEffects.None;
            if (Interlocked.Exchange(ref _dragEnterLogged, 1) == 0)
                AppHost.Get<ILogger<HomePage>>().LogInformation("First drag over the drop zone (files: {Files})", files);
        });
        DragDrop.AddDragOverHandler(DropZone, (_, e) =>
            e.DragEffects = DraggedFiles.Offered(e.DataTransfer) ? DragDropEffects.Copy : DragDropEffects.None);
        DragDrop.AddDragLeaveHandler(DropZone, (_, _) => DropZone.Classes.Remove("over"));
        DragDrop.AddDropHandler(DropZone, OnDrop);
    }

    private async void OnDropZoneDoubleTapped(object? sender, TappedEventArgs e)
    {
        e.Handled = true;
        if (TopLevel.GetTopLevel(this) is not { } top)
            return;

        var files = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { AllowMultiple = true });

        var paths = files
            .Select(f => f.TryGetLocalPath())
            .Where(p => !string.IsNullOrEmpty(p))
            .Select(p => p!)
            .ToList();

        if (paths.Count == 0)
            return;

        var dropZonePos = e.GetPosition(DropZone);
        var targetPos = new Rect(0, 0, DropZone.Bounds.Width, DropZone.Bounds.Height).Contains(dropZonePos)
            ? dropZonePos
            : new Point(DropZone.Bounds.Width / 2, DropZone.Bounds.Height / 2);
        var screenPoint = DropZone.PointToScreen(targetPos);
        AppHost.Get<RadialController>().ShowForFiles(screenPoint.X, screenPoint.Y, paths);
    }

    private async void OnExportHistoryClick(object? sender, RoutedEventArgs? e) => await ExportHistory(json: false);

    private async void OnExportJsonClick(object? sender, RoutedEventArgs? e) => await ExportHistory(json: true);

    private async Task ExportHistory(bool json)
    {
        if (TopLevel.GetTopLevel(this) is not { } top)
            return;
        var loc = AppHost.Get<ILocalizer>();
        var target = await top.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = loc[json ? "home.export_json" : "home.export_csv"],
            SuggestedFileName = json ? "morfa-history.json" : "morfa-history.csv",
            FileTypeChoices =
            [
                new FilePickerFileType(json ? "JSON" : "CSV")
                {
                    Patterns = [json ? "*.json" : "*.csv"],
                },
            ],
        });
        if (target?.TryGetLocalPath() is not { } path)
            return;
        var entries = AppHost.Get<UserDataStore>().History;
        if (entries.Count == 0)
            return; // nothing to write
        if (json)
            HistoryJson.Write(path, entries);
        else
            HistoryCsv.Write(path, entries);
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        DropZone.Classes.Remove("over");
        var dragged = DraggedFiles.Read(e.DataTransfer, forDrop: true);
        if (dragged.Files.Count == 0)
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        e.DragEffects = DragDropEffects.Copy;
        var screenPoint = this.PointToScreen(e.GetPosition(this));
        AppHost.Get<RadialController>().ShowForFiles(screenPoint.X, screenPoint.Y, dragged.Files, web: dragged.Web);
    }
}
