using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.VisualTree;
using Morfa.App.Controls;
using Morfa.App.Services;
using Morfa.App.ViewModels;
using Morfa.App.Views;
using Morfa.Core.Localization;

namespace Morfa.App.Tests;

public class LocalizationResourceTests
{
    [AvaloniaFact]
    public void Arabic_font_chain_draws_trimming_ellipsis_and_matches_arabic_letters()
    {
        // The font chain follows any OS culture, whether or not Morfa ships a translation for it.
        var family = FontFamily.Parse(ThemeService.FontFamilies("ar"));
        // Avalonia takes the trimming symbol from the primary typeface rather than applying character fallback.
        Assert.NotEqual((ushort)0, new Typeface(family).GlyphTypeface.CharacterToGlyphMap.GetGlyph(0x2026));
        Assert.True(FontManager.Current.TryMatchCharacter(0x0645, FontStyle.Normal, FontWeight.Normal,
            FontStretch.Normal, family, System.Globalization.CultureInfo.GetCultureInfo("ar"), out var arabic));
        Assert.NotEqual((ushort)0, arabic.GlyphTypeface.CharacterToGlyphMap.GetGlyph(0x0645));
    }

    [AvaloniaFact]
    public void A_right_to_left_culture_mirrors_the_main_window_and_a_left_to_right_one_brings_it_back()
    {
        TestServices.EnsureInitialized("en");
        using var model = new MainWindowViewModel(AppHost.Services, AppHost.Get<ILocalizer>(), AppHost.Get<UpdateService>());
        model.Navigate("general");
        var window = new MainWindow { DataContext = model, Width = 1080, Height = 820 };
        window.Show();
        try
        {
            RenderTests.Pump();
            var navigation = window.GetVisualDescendants().OfType<ListBox>().Single(l => l.Classes.Contains("nav"));
            var page = window.FindControl<ScrollViewer>("PageScroller")!;
            double Left(Visual visual) => new Rect(visual.Bounds.Size).TransformToAABB(visual.TransformToVisual(window)!.Value).Left;
            double Center(Visual visual) =>
                new Rect(visual.Bounds.Size).TransformToAABB(visual.TransformToVisual(window)!.Value).Center.X;
            Assert.Equal(FlowDirection.LeftToRight, window.FlowDirection);
            Assert.True(Center(navigation) < Center(page));
            var englishNavigationLeft = Left(navigation);

            // The flow direction follows the UI culture's script, applied through the same palette the app uses.
            SoftPalette.ApplyFlowDirection(Application.Current!.Resources, "ar");
            RenderTests.Pump();
            Assert.Equal(FlowDirection.RightToLeft, window.FlowDirection);
            Assert.Equal(FlowDirection.RightToLeft, page.FlowDirection);
            // The sidebar moves to the right edge, the page to its left.
            Assert.True(Center(navigation) > Center(page));
            Assert.Equal(window.Bounds.Width - englishNavigationLeft, Left(navigation) + navigation.Bounds.Width, 1);

            SoftPalette.ApplyFlowDirection(Application.Current!.Resources, "en");
            RenderTests.Pump();
            Assert.Equal(FlowDirection.LeftToRight, window.FlowDirection);
            Assert.Equal(englishNavigationLeft, Left(navigation), 1);
        }
        finally
        {
            window.Close();
            TestServices.EnsureInitialized("en");
        }
    }

    [AvaloniaFact]
    public void Right_to_left_windows_mirror_but_pictures_and_the_donut_do_not()
    {
        SoftPalette.ApplyFlowDirection(Application.Current!.Resources, "ar");
        var canvas = new CropCanvas();
        var donut = new DonutMenu();
        var picture = new AnimatedImage();
        var window = new Window { Content = new StackPanel { Children = { new TextBlock(), canvas, donut, picture } } };
        window.Show();
        try
        {
            Assert.Equal(FlowDirection.RightToLeft, window.FlowDirection);
            Assert.Equal(FlowDirection.RightToLeft, ((StackPanel)window.Content).Children[0].FlowDirection);
            Assert.Equal(FlowDirection.LeftToRight, canvas.FlowDirection);
            Assert.Equal(FlowDirection.LeftToRight, donut.FlowDirection);
            Assert.Equal(FlowDirection.LeftToRight, picture.FlowDirection);
        }
        finally
        {
            window.Close();
            TestServices.EnsureInitialized("en");
        }
    }

    [AvaloniaFact]
    public void Strings_are_available_as_application_resources()
    {
        TestServices.EnsureInitialized("id");
        Assert.True(Application.Current!.TryGetResource("home.title", null, out var value));
        Assert.Equal("Beranda", value);
    }

    [AvaloniaFact]
    public void DynamicResource_in_xaml_resolves_and_follows_language_changes()
    {
        TestServices.EnsureInitialized("en");
        var text = new TextBlock();
        text.Bind(TextBlock.TextProperty, text.GetResourceObservable("home.title"));
        var toggle = new ToggleSwitch();
        var window = new Window { Content = new StackPanel { Children = { text, toggle } } };
        window.Show();
        Assert.Equal("Home", text.Text);

        TestServices.EnsureInitialized("id");
        Assert.Equal("Beranda", text.Text);
        window.Close();
        TestServices.EnsureInitialized("en");
    }
}
