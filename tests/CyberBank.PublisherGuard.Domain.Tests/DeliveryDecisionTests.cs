using CyberBank.PublisherGuard.Domain.Evidence;
using CyberBank.PublisherGuard.Domain.Packages;

namespace CyberBank.PublisherGuard.Domain.Tests;

public sealed class DeliveryDecisionTests
{
    [Fact]
    public void Decision_preserves_a_relative_path_inclusion_reason_and_utc_timestamp()
    {
        var decidedAtUtc = new DateTimeOffset(2026, 8, 23, 14, 0, 0, TimeSpan.Zero);

        var decision = new DeliveryDecision(
            RelativePath: Path.Combine("assets", "sample.psd"),
            InclusionState: PackageInclusionState.Excluded,
            Reason: "Creator removed a draft source asset from the customer delivery.",
            DecidedAtUtc: decidedAtUtc);

        Assert.Equal(Path.Combine("assets", "sample.psd"), decision.RelativePath);
        Assert.Equal(PackageInclusionState.Excluded, decision.InclusionState);
        Assert.Equal("Creator removed a draft source asset from the customer delivery.", decision.Reason);
        Assert.Equal(decidedAtUtc, decision.DecidedAtUtc);
    }
}
