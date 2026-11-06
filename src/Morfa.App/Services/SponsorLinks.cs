// Where "Sponsor" can lead: the sidebar button and the About page open a window with these, the way that suits the
// The GitHub Sponsors page comes first.

namespace Morfa.App.Services;

/// <summary>One way to support Morfa.</summary>
/// <param name="Id">Also the suffix of its description string, <c>sponsor.note.{Id}</c>.</param>
/// <param name="Icon">Key of the logo geometry (Styles/Brands.axaml, or Icon.Heart).</param>
/// <param name="Color">Brand colour behind the white logo.</param>
public sealed record SponsorLink(string Id, string Name, string Url, string Icon, string Color);

public static class SponsorLinks
{
    public const string GitHubSponsorsUrl = "https://github.com/sponsors/nekonaru";
    public const string PatreonUrl = "https://patreon.com/nekonaru";
    public const string KoFiUrl = "https://ko-fi.com/nekonaru";
    public const string PayPalUrl = "https://paypal.me/nekonaru";

    private static readonly SponsorLink GitHub = new("github", "GitHub Sponsors", GitHubSponsorsUrl, "Brand.GitHubSponsors", "#BF3989");
    private static readonly SponsorLink Patreon = new("patreon", "Patreon", PatreonUrl, "Brand.Patreon", "#141518");
    private static readonly SponsorLink KoFi = new("kofi", "Ko-fi", KoFiUrl, "Brand.KoFi", "#FF5E5B");
    private static readonly SponsorLink PayPal = new("paypal", "PayPal", PayPalUrl, "Brand.PayPal", "#003087");
    private static readonly SponsorLink BuyMeACoffee = new("buymeacoffee", "Buy Me a Coffee", "https://buymeacoffee.com/nekonaru", "Icon.Heart", "#FFDD00");

    /// <summary>Every way to support the project.</summary>
    public static IReadOnlyList<SponsorLink> For(string language) =>
        [GitHub, Patreon, KoFi, PayPal, BuyMeACoffee];
}
