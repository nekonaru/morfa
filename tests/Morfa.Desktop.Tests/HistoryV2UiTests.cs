// F3 desktop tests: history filters, JSON export and pin/tags/notes editing.

using System.Text.Json;
using Avalonia.Headless.XUnit;
using Morfa.Desktop.Services;
using Morfa.Desktop.ViewModels.Pages;
using Morfa.Domain.Conversion;
using Morfa.Domain.History;
using Morfa.Domain.Settings;

namespace Morfa.Desktop.Tests;

public class HistoryV2UiTests
{
    [AvaloniaFact]
    public void History_filters_narrow_the_list_and_the_dashboard_uses_all_entries()
    {
        TestServices.EnsureInitialized("en");
        var store = AppHost.Get<UserDataStore>();
        store.History.Clear();
        store.AddHistory(new HistoryEntry
        {
            JobId = "1", PresetName = "To PNG", PresetId = "png", State = JobState.Completed,
            Sources = ["a.jpg"], Outputs = ["a.png"], FinishedAt = DateTime.Now,
        });
        store.AddHistory(new HistoryEntry
        {
            JobId = "2", PresetName = "To PDF", PresetId = "pdf", State = JobState.Failed,
            Sources = ["b.jpg"], Outputs = [], FinishedAt = DateTime.Now,
        });
        var vm = new HomePageViewModel(store, AppHost.Get<Morfa.Domain.Localization.ILocalizer>(),
            AppHost.Get<Morfa.Domain.Platform.IPlatformServices>());

        Assert.Equal(2, vm.Dashboard.Total);
        Assert.Equal(2, vm.History.Count);

        vm.FormatFilter = "png";
        Assert.Single(vm.History);
        Assert.Equal("To PNG", vm.History[0].Title);

        vm.FormatFilter = "";
        vm.StateFilter = "Failed";
        Assert.Single(vm.History);
        Assert.Equal("To PDF", vm.History[0].Title);

        vm.StateFilter = "";
        vm.Search = "client note";
        Assert.Empty(vm.History);
        vm.Dispose();
    }

    [AvaloniaFact]
    public void Pin_tags_and_notes_are_saved_without_reloading_the_page()
    {
        TestServices.EnsureInitialized("en");
        var store = AppHost.Get<UserDataStore>();
        store.History.Clear();
        store.AddHistory(new HistoryEntry
        {
            JobId = "3", PresetName = "To PNG", State = JobState.Completed,
            Sources = ["a.jpg"], Outputs = ["a.png"], FinishedAt = DateTime.Now,
        });
        var item = new HistoryItemViewModel(store.History[0], AppHost.Get<Morfa.Domain.Localization.ILocalizer>(),
            AppHost.Get<Morfa.Domain.Platform.IPlatformServices>(), store: store);

        item.TogglePinCommand.Execute(null);
        item.TagsText = "client, urgent";
        item.Notes = "Send on Friday";

        Assert.True(item.Pinned);
        var reloaded = new UserDataStore(store.Directory);
        reloaded.Load();
        Assert.True(reloaded.History[0].Pinned);
        Assert.Equal(["client", "urgent"], reloaded.History[0].Tags);
        Assert.Equal("Send on Friday", reloaded.History[0].Notes);
    }

    [Fact]
    public void Json_export_has_one_object_per_entry()
    {
        var dir = Directory.CreateTempSubdirectory("morfa-history-json-");
        var store = new UserDataStore(dir.FullName);
        store.Load();
        store.AddHistory(new HistoryEntry
        {
            JobId = "4", PresetName = "To PNG", State = JobState.Completed,
            Sources = ["a.jpg"], Outputs = ["a.png"], FinishedAt = DateTime.Now,
            Failures = [],
        });
        var path = Path.Combine(dir.FullName, "history.json");

        HistoryJson.Write(path, store.History);

        using var json = JsonDocument.Parse(File.ReadAllText(path));
        Assert.Equal(1, json.RootElement.GetArrayLength());
        var row = json.RootElement[0];
        Assert.Equal("To PNG", row.GetProperty("preset").GetString());
        Assert.Equal("Completed", row.GetProperty("state").GetString());
        Assert.Equal("a.png", row.GetProperty("outputs")[0].GetString());
        dir.Delete(recursive: true);
    }
}


