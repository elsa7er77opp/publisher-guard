using CyberBank.PublisherGuard.Domain.Packages;

namespace CyberBank.PublisherGuard.Domain.Tests;

public sealed class PackagePresetTests
{
    [Fact]
    public void Standard_asset_pack_preset_has_plain_language_identity_and_default_profile()
    {
        var preset = PackagePreset.StandardAssetPack;

        Assert.Equal("standard-asset-pack", preset.Id);
        Assert.Equal("Standard Asset Pack", preset.Name);
        Assert.Equal(PackageProfile.Default, preset.Profile);
        Assert.Equal("Checks a typical template or digital asset delivery.", preset.Description);
    }
}
