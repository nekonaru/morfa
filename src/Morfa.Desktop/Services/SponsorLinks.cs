// Where "Sponsor" can lead: the sidebar button and the About page open a window with these, the way that suits the
// user first, then the others. GitHub Sponsors comes first.

namespace Morfa.Desktop.Services;

/// <summary>One way to support Morfa.</summary>
/// <param name="Id">Also the suffix of its description string, <c>sponsor.note.{Id}</c>.</param>
/// <param name="Icon">Key of the logo geometry (Styles/Brands.axaml, or Icon.Heart).</param>
/// <param name="Color">Brand colour behind the white logo.</param>
public sealed record SponsorLink(string Id, string Name, string Url, string Icon, string Color);

public static class SponsorLinks
{
    public const string GitHubSponsorsUrl = "https://github.com/sponsors/nekonaru";
    public const string SaweriaUrl = "https://saweria.co/nekonaruu";
    public const string SociaBuzzUrl = "https://sociabuzz.com/nekonaru/donate";
    public const string BagibagiUrl = "https://bagibagi.co/Nekonaru";
    public const string BuyMeACoffeeUrl = "https://buymeacoffee.com/nekonaru";

    private static readonly SponsorLink GitHub = new("github", "GitHub Sponsors", GitHubSponsorsUrl, "Brand.GitHubSponsors", "#BF3989");
    private static readonly SponsorLink Saweria = new("saweria", "Saweria", SaweriaUrl, "Icon.Heart", "#E5487B");
    private static readonly SponsorLink SociaBuzz = new("sociabuzz", "SociaBuzz", SociaBuzzUrl, "Icon.Heart", "#46A758");
    private static readonly SponsorLink Bagibagi = new("bagibagi", "Bagibagi", BagibagiUrl, "Icon.Heart", "#F76B15");
    private static readonly SponsorLink BuyMeACoffee = new("buymeacoffee", "Buy Me a Coffee", BuyMeACoffeeUrl, "Icon.Heart", "#FFDD00");

    /// <summary>Every way to support the project.</summary>
    public static IReadOnlyList<SponsorLink> For(string language) =>
        [GitHub, Saweria, SociaBuzz, Bagibagi, BuyMeACoffee];
}
