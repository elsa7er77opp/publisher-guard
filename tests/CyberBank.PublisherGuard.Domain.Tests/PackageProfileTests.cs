using CyberBank.PublisherGuard.Domain.Packages;

namespace CyberBank.PublisherGuard.Domain.Tests;

public sealed class PackageProfileTests
{
    [Fact]
    public void Default_profile_is_DigitalTemplateAssetPack()
    {
        Assert.Equal(PackageProfile.DigitalTemplateAssetPack, PackageProfile.Default);
    }
}
