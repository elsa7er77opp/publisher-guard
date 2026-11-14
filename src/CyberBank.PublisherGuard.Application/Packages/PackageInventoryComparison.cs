namespace CyberBank.PublisherGuard.Application.Packages;

public sealed record PackageInventoryComparison(
    IReadOnlyList<string> Added,
    IReadOnlyList<string> Removed,
    IReadOnlyList<string> Changed);
