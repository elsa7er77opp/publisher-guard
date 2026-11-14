using CyberBank.PublisherGuard.Application.Delivery;
using CyberBank.PublisherGuard.Domain.Evidence;
using CyberBank.PublisherGuard.Domain.Packages;

namespace CyberBank.PublisherGuard.Application.Tests;

public sealed class CleanPackageBuildQueueTests
{
    [Fact]
    public async Task EnqueueAsync_when_cancelled_before_execution_does_not_invoke_the_builder()
    {
        var builder = new CountingBuilder();
        var queue = new CleanPackageBuildQueue(builder);
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();
        var command = new CleanPackageBuildCommand(
            SourceRoot: @"C:\source",
            OutputParent: @"C:\output",
            DeliveryName: "clean-package",
            Manifest: CleanPackageManifest.Create(PackageProfile.Default, Array.Empty<PackageInventoryEntry>()),
            Decisions: Array.Empty<DeliveryDecision>());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => queue.EnqueueAsync(command, cancellationSource.Token));

        Assert.Equal(0, builder.CallCount);
    }

    private sealed class CountingBuilder : ICleanPackageBuilder
    {
        public int CallCount { get; private set; }

        public CleanPackageBuildOutcome Build(
            string sourceRoot,
            string outputParent,
            string deliveryName,
            CleanPackageManifest manifest,
            IReadOnlyList<DeliveryDecision> decisions,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return new CleanPackageBuildOutcome(true, Path.Combine(outputParent, deliveryName));
        }
    }
}
