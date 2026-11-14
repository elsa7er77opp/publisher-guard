namespace CyberBank.PublisherGuard.Domain.Packages;

public sealed record PackageProfile(string Id)
{
    public static readonly PackageProfile DigitalTemplateAssetPack = new("digital-template-asset-pack");

    public static PackageProfile Default => DigitalTemplateAssetPack;
}
