using CyberBank.PublisherGuard.Application.Packages;
using CyberBank.PublisherGuard.Domain.Evidence;
using CyberBank.PublisherGuard.Domain.Packages;

namespace CyberBank.PublisherGuard.Application.Tests;

public sealed class PackageInclusionDecisionServiceTests
{
    [Fact]
    public void Apply_excludes_the_requested_relative_path_with_a_reviewable_reason()
    {
        var entries = new[]
        {
            new PackageInventoryEntry("assets/logo.png", PackageFileType.Binary, 32, "AA", PackageInclusionState.Included, null, ["absolute-local-path"])
        };
        var decision = new DeliveryDecision(
            RelativePath: "assets/logo.png",
            InclusionState: PackageInclusionState.Excluded,
            Reason: "Creator excluded this draft asset from delivery.",
            DecidedAtUtc: DateTimeOffset.UtcNow);

        var updatedEntry = Assert.Single(PackageInclusionDecisionService.Apply(entries, decision));

        Assert.Equal(PackageInclusionState.Excluded, updatedEntry.InclusionState);
        Assert.Equal("Creator excluded this draft asset from delivery.", updatedEntry.Reason);
        Assert.Equal("AA", updatedEntry.Sha256);
        Assert.Equal("absolute-local-path", Assert.Single(updatedEntry.MetadataSignals!));
    }
}
