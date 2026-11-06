// "Sponsor" opens a window with every way to support Morfa, the one that suits the language first: Fairy (Korean cards
// and easy-pay) in Korean, GitHub Sponsors otherwise.

using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Morfa.App.Services;
using Morfa.App.Views;
using Morfa.Core.Localization;

namespace Morfa.App.Tests;

public class SponsorTests
{
    [AvaloniaTheory]
    [InlineData("id", SponsorLinks.GitHubSponsorsUrl)]
    [InlineData("en", SponsorLinks.GitHubSponsorsUrl)]
    public void The_way_that_suits_the_language_comes_first(string language, string url)
    {
        var ways = SponsorLinks.For(language);

        Assert.Equal(url, ways[0].Url);
        Assert.Equal(5, ways.Select(w => w.Url).Distinct().Count());
    }

    [AvaloniaTheory]
    [InlineData("en")]
    [InlineData("id")]
    public void Every_way_has_its_logo_and_note(string language)
    {
        TestServices.EnsureInitialized(language);
        var window = SponsorWindow.Create(AppHost.Get<ILocalizer>());

        var ways = Assert.IsAssignableFrom<IEnumerable<SponsorWay>>(window.Ways.ItemsSource).ToList();
        Assert.Equal(5, ways.Count);
        Assert.All(ways, w =>
        {
            Assert.IsAssignableFrom<Geometry>(w.Icon);
            Assert.False(string.IsNullOrWhiteSpace(w.Note));
            Assert.DoesNotContain("sponsor.note.", w.Note);
        });
    }
}
