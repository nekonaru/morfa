// F1: keyboard navigation and type-to-filter in the live donut.

using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using Morfa.Desktop.Controls;
using Morfa.Desktop.Services;
using Morfa.Desktop.ViewModels;
using Morfa.Desktop.Views;
using Morfa.Domain.History;
using Morfa.Domain.Localization;
using Morfa.Domain.Settings;

namespace Morfa.Desktop.Tests;

public class DonutKeyboardFilterTests
{
    private static DonutItem Item(string label) => new() { Id = label, Label = label };

    [AvaloniaFact]
    public void Arrow_keys_move_the_selection_and_enter_invokes_it_once()
    {
        TestServices.EnsureInitialized("en");
        var donut = new DonutMenu
        {
            Items = [Item("PNG"), Item("JPG"), Item("WEBP")],
        };
        var window = new Window { Content = donut, SizeToContent = SizeToContent.WidthAndHeight };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        var invoked = new List<int>();
        donut.SegmentInvoked += (_, index) => invoked.Add(index);

        donut.HandleKey(Key.Right);
        var first = donut.ActiveIndex;
        donut.HandleKey(Key.Right);
        Assert.Equal((first + 1) % 3, donut.ActiveIndex);
        donut.HandleKey(Key.Left);
        Assert.Equal(first, donut.ActiveIndex);
        donut.HandleKey(Key.Enter);

        Assert.Equal([first], invoked);
        window.Close();
    }

    [Fact]
    public void Recently_used_presets_are_promoted_without_losing_any()
    {
        var ids = new[] { "to-webp", "to-png", "to-jpg" };
        var history = new[]
        {
            new HistoryEntry { PresetId = "to-png", FinishedAt = DateTime.Now, State = Morfa.Domain.Conversion.JobState.Completed },
            new HistoryEntry { PresetId = "to-jpg", FinishedAt = DateTime.Now.AddMinutes(1), State = Morfa.Domain.Conversion.JobState.Failed },
        };

        var ordered = RadialViewModel.OrderByRecent(ids, history);

        Assert.Equal("to-png", ordered[0]);
        Assert.Equal(3, ordered.Count);
        Assert.Equal(["to-webp", "to-jpg"], ordered.Skip(1));
    }

    [AvaloniaFact]
    public void Typing_a_letter_narrows_the_slices_and_backspace_restores_them()
    {
        TestServices.EnsureInitialized("en");
        var vm = NewViewModel();
        var folder = Directory.CreateTempSubdirectory("morfa-filter-");
        try
        {
            var png = Path.Combine(folder.FullName, "a.png");
            File.WriteAllText(png, "x");
            vm.Load([png], clickMode: true);
            var all = vm.Items.Count;
            Assert.True(all > 1);

            Assert.True(vm.SetFilter("png"));
            Assert.True(vm.Items.Count > 0 && vm.Items.Count < all);
            Assert.All(vm.Items, item =>
                Assert.True(item.Label.Contains("png", StringComparison.OrdinalIgnoreCase)
                    || item.Id.Contains("png", StringComparison.OrdinalIgnoreCase)));
            Assert.Equal("png", vm.Filter);

            vm.SetFilter("pn");
            Assert.True(vm.Filter.Length == 2);
            vm.SetFilter(vm.Filter[..^1]);
            Assert.True(vm.Filter.Length == 1);

            vm.SetFilter("");
            Assert.Equal(all, vm.Items.Count);
            Assert.Equal("", vm.Filter);
        }
        finally
        {
            folder.Delete(recursive: true);
        }
    }

    [AvaloniaFact]
    public void Filtering_applies_to_the_back_face_and_no_match_is_reported_to_the_hub()
    {
        TestServices.EnsureInitialized("en");
        var vm = NewViewModel();
        var folder = Directory.CreateTempSubdirectory("morfa-filter-");
        try
        {
            var png = Path.Combine(folder.FullName, "a.png");
            File.WriteAllText(png, "x");
            vm.Load([png], clickMode: true);

            if (vm.CanFlip)
                Assert.True(vm.ToggleFace());
            var visible = vm.Items.Count;

            Assert.False(vm.SetFilter("no-such-format"));
            Assert.Empty(vm.Items);
            vm.ShowHint(-1, -1, false);
            Assert.Equal("No matching format", vm.CenterSubtitle);

            vm.SetFilter("");
            Assert.Equal(visible, vm.Items.Count);
        }
        finally
        {
            folder.Delete(recursive: true);
        }
    }

    [AvaloniaFact]
    public void Modifier_shortcuts_do_not_become_filter_text()
    {
        TestServices.EnsureInitialized("en");
        var vm = NewViewModel();
        var folder = Directory.CreateTempSubdirectory("morfa-filter-");
        try
        {
            var png = Path.Combine(folder.FullName, "a.png");
            File.WriteAllText(png, "x");
            vm.Load([png], clickMode: true);
            var window = new RadialWindow { DataContext = vm };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.Focus();

            window.KeyPress(Key.P, RawInputModifiers.Control, PhysicalKey.P, null);

            Assert.Equal("", vm.Filter);
            window.Close();
        }
        finally
        {
            folder.Delete(recursive: true);
        }
    }

    [AvaloniaFact]
    public void Escape_closes_when_the_filter_is_empty()
    {
        TestServices.EnsureInitialized("en");
        var vm = NewViewModel();
        var folder = Directory.CreateTempSubdirectory("morfa-filter-");
        try
        {
            var png = Path.Combine(folder.FullName, "a.png");
            File.WriteAllText(png, "x");
            vm.Load([png], clickMode: true);
            var window = new RadialWindow { DataContext = vm };
            var closed = 0;
            window.EscapePressed += () => closed++;
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.Focus();

            window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);

            Assert.Equal("", vm.Filter);
            Assert.Equal(1, closed);
            window.Close();
        }
        finally
        {
            folder.Delete(recursive: true);
        }
    }

    [AvaloniaFact]
    public void Escape_clears_the_filter_before_the_window_closes()
    {
        TestServices.EnsureInitialized("en");
        var vm = NewViewModel();
        var folder = Directory.CreateTempSubdirectory("morfa-filter-");
        try
        {
            var png = Path.Combine(folder.FullName, "a.png");
            File.WriteAllText(png, "x");
            vm.Load([png], clickMode: true);
            var window = new RadialWindow { DataContext = vm };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.Focus();

            window.KeyPress(Key.P, RawInputModifiers.None, PhysicalKey.P, null);
            window.KeyPress(Key.N, RawInputModifiers.None, PhysicalKey.N, null);
            Assert.Equal("pn", vm.Filter);

            window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            Assert.Equal("", vm.Filter);
            window.Close();
        }
        finally
        {
            folder.Delete(recursive: true);
        }
    }

    private static RadialViewModel NewViewModel() => new(
        AppHost.Get<UserDataStore>(),
        AppHost.Get<ILocalizer>(),
        AppHost.Get<PresetAvailability>());
}
