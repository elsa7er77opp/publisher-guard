using CyberBank.PublisherGuard.Domain.Packages;
using CyberBank.PublisherGuard.Domain.Preflight;

namespace CyberBank.PublisherGuard.Application.Packages;

public sealed record PackageWorkflowSnapshot(
    bool IsAuthorized,
    string SourceRoot,
    string OutputParent,
    IReadOnlyList<PackageInventoryEntry> Entries,
    IReadOnlyList<PreflightIssue> Issues);
