using System.Text.RegularExpressions;
using CyberBank.PublisherGuard.Domain.Packages;

namespace CyberBank.PublisherGuard.Application.Packages;

public static class LocalIgnoreRuleApplier
{
    public static IReadOnlyList<PackageInventoryEntry> Apply(
        IReadOnlyList<PackageInventoryEntry> entries,
        IReadOnlyList<LocalIgnoreRule> rules)
    {
        return entries.Select(entry => Apply(entry, rules)).ToArray();
    }

    private static PackageInventoryEntry Apply(PackageInventoryEntry entry, IReadOnlyList<LocalIgnoreRule> rules)
    {
        if (entry.InclusionState != PackageInclusionState.Included)
        {
            return entry;
        }

        var matchingRule = rules.FirstOrDefault(rule => Matches(entry.RelativePath, rule));
        return matchingRule is null
            ? entry
            : entry with { InclusionState = PackageInclusionState.Excluded, Reason = matchingRule.Reason };
    }

    private static bool Matches(string relativePath, LocalIgnoreRule rule)
    {
        return rule.Kind switch
        {
            LocalIgnoreRuleKind.RelativePath => relativePath.Equals(rule.Pattern, StringComparison.OrdinalIgnoreCase),
            LocalIgnoreRuleKind.FileNamePattern => Regex.IsMatch(
                Path.GetFileName(relativePath),
                "^" + Regex.Escape(rule.Pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
            _ => false
        };
    }
}
