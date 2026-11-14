using CyberBank.PublisherGuard.Domain.Evidence;
using CyberBank.PublisherGuard.Domain.Packages;

namespace CyberBank.PublisherGuard.Application.Packages;

public static class PackageInclusionDecisionService
{
    public static IReadOnlyList<PackageInventoryEntry> Apply(
        IReadOnlyList<PackageInventoryEntry> entries,
        DeliveryDecision decision)
    {
        return entries
            .Select(entry => entry.RelativePath.Equals(decision.RelativePath, StringComparison.OrdinalIgnoreCase)
                ? entry with
                {
                    InclusionState = decision.InclusionState,
                    Reason = decision.Reason
                }
                : entry)
            .ToArray();
    }
}
