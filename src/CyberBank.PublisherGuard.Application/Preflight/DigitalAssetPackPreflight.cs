using CyberBank.PublisherGuard.Domain.Packages;
using CyberBank.PublisherGuard.Domain.Preflight;

namespace CyberBank.PublisherGuard.Application.Preflight;

public static class DigitalAssetPackPreflight
{
    public static IReadOnlyList<PreflightIssue> Run(IEnumerable<PackageInventoryEntry> entries)
    {
        var inventory = entries.ToArray();

        if (inventory.Length == 0)
        {
            return
            [
                new PreflightIssue(
                    RuleId: "PG-PACKAGE-001",
                    Severity: PreflightSeverity.Blocked,
                    RelativePath: ".",
                    Explanation: "The selected package does not contain any files.")
            ];
        }

        var hasReadme = inventory.Any(entry =>
            entry.InclusionState == PackageInclusionState.Included
            && Path.GetFileName(entry.RelativePath).Equals("README.md", StringComparison.OrdinalIgnoreCase));
        var issues = new List<PreflightIssue>();

        foreach (var duplicate in inventory
                     .Where(entry => entry.InclusionState == PackageInclusionState.Included)
                     .GroupBy(entry => entry.RelativePath, StringComparer.OrdinalIgnoreCase)
                     .Where(group => group.Count() > 1)
                     .SelectMany(group => group.Skip(1)))
        {
            issues.Add(new PreflightIssue(
                RuleId: "PG-PATH-021",
                Severity: PreflightSeverity.ReviewRequired,
                RelativePath: duplicate.RelativePath,
                Explanation: "Another included entry resolves to the same path on a case-insensitive file system."));
        }

        foreach (var entryWithAbsolutePathMetadata in inventory.Where(entry =>
                     entry.InclusionState == PackageInclusionState.Included
                     && entry.MetadataSignals?.Any(signal => signal == "absolute-local-path") == true))
        {
            issues.Add(new PreflightIssue(
                RuleId: "PG-METADATA-040",
                Severity: PreflightSeverity.ReviewRequired,
                RelativePath: entryWithAbsolutePathMetadata.RelativePath,
                Explanation: "Text metadata appears to contain an absolute local path and needs human review."));
        }

        if (!hasReadme)
        {
            issues.Add(new PreflightIssue(
                RuleId: "PG-REQUIRED-010",
                Severity: PreflightSeverity.ReviewRequired,
                RelativePath: "README.md",
                Explanation: "A creator README is required for human review."));
        }

        return issues;
    }
}
