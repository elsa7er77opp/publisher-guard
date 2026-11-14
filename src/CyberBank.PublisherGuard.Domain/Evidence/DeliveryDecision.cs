using CyberBank.PublisherGuard.Domain.Packages;

namespace CyberBank.PublisherGuard.Domain.Evidence;

public sealed record DeliveryDecision(
    string RelativePath,
    PackageInclusionState InclusionState,
    string Reason,
    DateTimeOffset DecidedAtUtc);
