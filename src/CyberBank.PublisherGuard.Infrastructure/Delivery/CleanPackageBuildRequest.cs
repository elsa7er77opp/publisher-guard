using CyberBank.PublisherGuard.Domain.Evidence;

namespace CyberBank.PublisherGuard.Infrastructure.Delivery;

public sealed record CleanPackageBuildRequest(
    string SourceRoot,
    string OutputParent,
    string DeliveryName,
    CleanPackageManifest Manifest,
    IReadOnlyList<DeliveryDecision>? Decisions = null);
