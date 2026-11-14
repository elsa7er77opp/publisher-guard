using CyberBank.PublisherGuard.Application.Packages;
using CyberBank.PublisherGuard.Domain.Packages;

namespace CyberBank.PublisherGuard.Application.Tests;

public sealed class PackageInventoryComparerTests
{
    [Fact]
    public void Compare_separates_added_removed_and_sha_changed_relative_paths()
    {
        var before = new[]
        {
            new PackageInventoryEntry("README.md", PackageFileType.Text, 4, "AAAA", PackageInclusionState.Included, null),
            new PackageInventoryEntry("old-preview.png", PackageFileType.Binary, 8, "BBBB", PackageInclusionState.Included, null)
        };
        var after = new[]
        {
            new PackageInventoryEntry("README.md", PackageFileType.Text, 5, "CCCC", PackageInclusionState.Included, null),
            new PackageInventoryEntry("new-preview.png", PackageFileType.Binary, 8, "DDDD", PackageInclusionState.Included, null)
        };

        var comparison = PackageInventoryComparer.Compare(before, after);

        Assert.Equal(["new-preview.png"], comparison.Added);
        Assert.Equal(["old-preview.png"], comparison.Removed);
        Assert.Equal(["README.md"], comparison.Changed);
    }
}
