// Main (settings) window: left navigation + current page.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Morfa.Desktop.Services;
using Morfa.Desktop.ViewModels.Pages;
using Morfa.Domain.Localization;

namespace Morfa.Desktop.ViewModels;

/// <summary>Navigation entry. <see cref="Title"/> follows the UI language.</summary>
public sealed partial class NavItem(string key, string titleKey, string iconKey, Type pageType, string group) : ObservableObject
{
    public string Key { get; } = key;
    public string TitleKey { get; } = titleKey;
    public Geometry? Icon { get; } = Application.Current?.FindResource(iconKey) as Geometry;
    public Type PageType { get; } = pageType;
    /// <summary>Top-bar group the item belongs to (see <see cref="NavGroup"/>).</summary>
    public string Group { get; } = group;

    [ObservableProperty] private string _title = "";

    public void Refresh(ILocalizer loc) => Title = loc[TitleKey];
}

/// <summary>One pill of the top bar row: a group of pages shown in the row below.</summary>
public sealed record NavGroup(string Key, string TitleKey, IReadOnlyList<NavItem> Items)
{
    public string Title { get; set; } = "";
    public void Refresh(ILocalizer loc) => Title = loc[TitleKey];
}

/// <summary>
/// One per main window (<see cref="WindowService.ShowMain"/>); disposed with it, which drops its subscriptions on the
/// long-lived services and the current page.
/// </summary>
public sealed partial class MainWindowViewModel : ObservableObject, IDisposable
{
    private readonly IServiceProvider _services;
    private readonly ILocalizer _loc;
    private bool _disposed;

    /// <summary>The repository, for the GitHub button above the version.</summary>
    public string RepositoryUrl => UpdateService.RepositoryUrl;

    public MainWindowViewModel(IServiceProvider services, ILocalizer loc, UpdateService updates)
    {
        _services = services;
        _loc = loc;
        Updates = updates;
        updates.PropertyChanged += OnUpdatesChanged;
        RefreshVersionTexts();
        Items =
        [
            new("home", "nav.home", "Icon.Home", typeof(HomePageViewModel), "home"),
            new("general", "nav.general", "Icon.Settings", typeof(GeneralPageViewModel), "app"),
            new("triggers", "nav.triggers", "Icon.Keyboard", typeof(TriggersPageViewModel), "donut"),
            new("toolbar", "nav.toolbar", "Icon.Donut", typeof(ToolbarPageViewModel), "donut"),
            new("presets", "nav.presets", "Icon.Tune", typeof(PresetsPageViewModel), "donut"),
            new("watch", "nav.watch", "Icon.Folder", typeof(WatchFoldersPageViewModel), "conversion"),
            new("theme", "nav.theme", "Icon.Palette", typeof(ThemePageViewModel), "app"),
            new("engines", "nav.engines", "Icon.Engine", typeof(EnginesPageViewModel), "conversion"),
            new("about", "nav.about", "Icon.Info", typeof(AboutPageViewModel), "app"),
        ];
        foreach (var item in Items)
            item.Refresh(loc);
        Groups =
        [
            new("home", "nav.group.home", Items.Where(i => i.Group == "home").ToArray()),
            new("donut", "nav.group.donut", Items.Where(i => i.Group == "donut").ToArray()),
            new("conversion", "nav.group.conversion", Items.Where(i => i.Group == "conversion").ToArray()),
            new("app", "nav.group.app", Items.Where(i => i.Group == "app").ToArray()),
        ];
        foreach (var group in Groups)
            group.Refresh(loc);
        loc.LanguageChanged += OnLanguageChanged;
        _selectedGroup = Groups[0];
        _selectedItem = Items[0];
        _currentPage = CreatePage(Items[0]);
    }

    public IReadOnlyList<NavItem> Items { get; }

    /// <summary>The top-bar groups, in display order (see each group's Items).</summary>
    public IReadOnlyList<NavGroup> Groups { get; }

    /// <summary>Update state for the top bar (version, "update available" button).</summary>
    public UpdateService Updates { get; }

    /// <summary>The page list the sub-nav row shows (the selected group's items).</summary>
    public IReadOnlyList<NavItem> SubItems => SelectedGroup.Items;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubItems))]
    private NavGroup _selectedGroup;

    // The sub-page shown last in each group, restored when the group is picked again.
    private readonly Dictionary<string, NavItem> _lastByGroup = new();

    [ObservableProperty] private NavItem _selectedItem;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageScroll))]
    private ObservableObject _currentPage;

    /// <summary>
    /// Pages scroll as a whole, except pages that fit the window height and scroll their own parts
    /// (<see cref="IFitsWindowHeight"/>, e.g. the presets list and editor).
    /// </summary>
    public ScrollBarVisibility PageScroll => CurrentPage is IFitsWindowHeight ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;
    [ObservableProperty] private string _versionText = "";
    [ObservableProperty] private string _updateText = "";
    [ObservableProperty] private string _updateTip = "";

    /// <summary>Downloads and runs the new installer (or opens the download page in a portable copy).</summary>
    [RelayCommand]
    private Task DownloadUpdate() => AppHost.Get<WindowService>().StartUpdateAsync(Updates);

    private void OnUpdatesChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (UpdateTexts.Affects(e.PropertyName))
            RefreshVersionTexts();
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        foreach (var item in Items)
            item.Refresh(_loc);
        foreach (var group in Groups)
            group.Refresh(_loc);
        RefreshVersionTexts();
        CurrentPage = CreatePage(SelectedItem); // rebuild so code-generated labels update too
    }

    private void RefreshVersionTexts()
    {
        VersionText = _loc.Format("about.version", UpdateService.CurrentVersion);
        UpdateText = UpdateTexts.Short(Updates, _loc);
        var changes = UpdateTexts.Changes(Updates, _loc);
        UpdateTip = UpdateTexts.Status(Updates, _loc)
                    ?? $"{_loc[$"update.level.{Updates.Level}"]}{Environment.NewLine}{(changes.Length > 0 ? changes + Environment.NewLine : "")}{_loc[UpdateTexts.HowKey]}";
    }

    public void Navigate(string key)
    {
        var item = Items.FirstOrDefault(i => i.Key == key);
        if (item is not null)
            SelectedItem = item;
    }

    /// <summary>Picks a group, reopening its last visited sub-page.</summary>
    public void SelectGroup(NavGroup? group)
    {
        if (group is null || group == SelectedGroup)
            return;
        SelectedGroup = group;
        if (_lastByGroup.TryGetValue(group.Key, out var last))
            SelectedItem = last;
    }

    // Pages are recreated on every visit so they always reflect the latest settings. A navigation list that loses its
    // selection keeps the open page (Choices.Restore).
    partial void OnSelectedItemChanged(NavItem? oldValue, NavItem newValue)
    {
        if (newValue is null)
        {
            Choices.Restore(oldValue, newValue, v => SelectedItem = v);
            return;
        }
        _lastByGroup[newValue.Group] = newValue;
        if (SelectedGroup.Key != newValue.Group)
            SelectedGroup = Groups.First(g => g.Key == newValue.Group);
        CurrentPage = CreatePage(newValue);
    }

    // Old pages may hold event subscriptions on long-lived services.
    partial void OnCurrentPageChanged(ObservableObject? oldValue, ObservableObject newValue) => (oldValue as IDisposable)?.Dispose();

    private ObservableObject CreatePage(NavItem item) =>
        (ObservableObject)ActivatorUtilities.CreateInstance(_services, item.PageType);

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        Updates.PropertyChanged -= OnUpdatesChanged;
        _loc.LanguageChanged -= OnLanguageChanged;
        (CurrentPage as IDisposable)?.Dispose();
    }
}
