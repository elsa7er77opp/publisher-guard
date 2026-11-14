using CyberBank.PublisherGuard.Domain.Evidence;
using CyberBank.PublisherGuard.Domain.Packages;

namespace CyberBank.PublisherGuard.Domain.Tests;

public sealed class CleanPackageManifestTests
{
    [Fact]
    public void Create_omits_entries_excluded_from_final_delivery()
    {
        var finalEntries = new[]
        {
            new PackageInventoryEntry("included.txt", PackageFileType.Text, 1, "AA", PackageInclusionState.Included, null),
            new PackageInventoryEntry("Thumbs.db", PackageFileType.Binary, 1, "BB", PackageInclusionState.Excluded, "PG-NAME-020")
        };

        var manifest = CleanPackageManifest.Create(PackageProfile.Default, finalEntries);

        var entry = Assert.Single(manifest.Entries);
        Assert.Equal("included.txt", entry.RelativePath);
    }

    [Fact]
    public void Create_keeps_only_final_relative_entry_metadata()
    {
        var finalEntries = new[]
        {
            new PackageInventoryEntry(
                RelativePath: Path.Combine("templates", "landing-page.json"),
                FileType: PackageFileType.Text,
                ByteLength: 128,
                Sha256: "ABCD",
                InclusionState: PackageInclusionState.Included,
                Reason: null)
        };

        var manifest = CleanPackageManifest.Create(PackageProfile.Default, finalEntries);

        Assert.Equal(PackageProfile.DigitalTemplateAssetPack, manifest.Profile);
        var entry = Assert.Single(manifest.Entries);
        Assert.Equal(Path.Combine("templates", "landing-page.json"), entry.RelativePath);
        Assert.Equal("ABCD", entry.Sha256);
        Assert.DoesNotContain("C:", entry.RelativePath, StringComparison.OrdinalIgnoreCase);
    }
}
