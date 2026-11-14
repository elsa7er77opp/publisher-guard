using CyberBank.PublisherGuard.Domain.Evidence;

namespace CyberBank.PublisherGuard.Application.Delivery;

public interface ICleanPackageBuilder
{
    CleanPackageBuildOutcome Build(
        string sourceRoot,
        string outputParent,
        string deliveryName,
        CleanPackageManifest manifest,
        IReadOnlyList<DeliveryDecision> decisions,
        CancellationToken cancellationToken = default);
}
