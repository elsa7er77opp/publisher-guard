using System.Threading;
using CyberBank.PublisherGuard.Application.Delivery;
using CyberBank.PublisherGuard.Domain.Evidence;

namespace CyberBank.PublisherGuard.Infrastructure.Delivery;

public sealed class LocalCleanPackageBuilder : ICleanPackageBuilder
{
    public CleanPackageBuildOutcome Build(
        string sourceRoot,
        string outputParent,
        string deliveryName,
        CleanPackageManifest manifest,
        IReadOnlyList<DeliveryDecision> decisions,
        CancellationToken cancellationToken = default)
    {
        var result = AtomicCleanPackageBuilder.Build(new CleanPackageBuildRequest(sourceRoot, outputParent, deliveryName, manifest, decisions), cancellationToken);
        return new CleanPackageBuildOutcome(result.IsComplete, result.OutputRoot);
    }
}
