using CyberBank.PublisherGuard.Domain.Evidence;

namespace CyberBank.PublisherGuard.Application.Delivery;

public sealed record CleanPackageBuildCommand(
    string SourceRoot,
    string OutputParent,
    string DeliveryName,
    CleanPackageManifest Manifest,
    IReadOnlyList<DeliveryDecision> Decisions);
