using CyberBank.PublisherGuard.Application.Packages;
using CyberBank.PublisherGuard.Domain.Packages;

namespace CyberBank.PublisherGuard.Application.Tests;

public sealed class LocalIgnoreRuleApplierTests
{
    [Fact]
    public void Apply_excludes_matching_included_file_and_preserves_automatic_exclusion()
    {
        var entries = new[]
        {
            new PackageInventoryEntry("draft.tmp", PackageFileType.Text, 4, "AAAA", PackageInclusionState.Included, null),
            new PackageInventoryEntry("helper.exe", PackageFileType.Executable, 8, "BBBB", PackageInclusionState.Excluded, "PG-CONTENT-050")
        };
        var rules = new[]
        {
            new LocalIgnoreRule(LocalIgnoreRuleKind.FileNamePattern, "*.tmp", "Creator ignores temporary drafts.", DateTimeOffset.UtcNow)
        };

        var applied = LocalIgnoreRuleApplier.Apply(entries, rules);

        var ignored = applied.Single(entry => entry.RelativePath == "draft.tmp");
        Assert.Equal(PackageInclusionState.Excluded, ignored.InclusionState);
        Assert.Equal("Creator ignores temporary drafts.", ignored.Reason);
        Assert.Equal("AAAA", ignored.Sha256);

        var executable = applied.Single(entry => entry.RelativePath == "helper.exe");
        Assert.Equal(PackageInclusionState.Excluded, executable.InclusionState);
        Assert.Equal("PG-CONTENT-050", executable.Reason);
    }
}
