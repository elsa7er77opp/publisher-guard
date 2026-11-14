namespace CyberBank.PublisherGuard.Domain.Packages;

public sealed record PackagePreset(string Id, string Name, string Description, PackageProfile Profile)
{
    public static readonly PackagePreset StandardAssetPack = new(
        "standard-asset-pack",
        "Standard Asset Pack",
        "Checks a typical template or digital asset delivery.",
        PackageProfile.Default);
}
