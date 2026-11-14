using CyberBank.PublisherGuard.Application.Preflight;
using CyberBank.PublisherGuard.Domain.Packages;
using CyberBank.PublisherGuard.Domain.Preflight;

namespace CyberBank.PublisherGuard.Application.Packages;

public sealed class PackageWorkflowCoordinator
{
    private readonly IPackageInventory inventory;
    private readonly IReadOnlyList<LocalIgnoreRule>? fixedIgnoreRules;
    private readonly ILocalIgnoreRuleStore? ignoreRuleStore;

    public PackageWorkflowCoordinator(IPackageInventory inventory)
    {
        this.inventory = inventory;
    }

    public PackageWorkflowCoordinator(IPackageInventory inventory, IReadOnlyList<LocalIgnoreRule> localIgnoreRules)
    {
        this.inventory = inventory;
        fixedIgnoreRules = localIgnoreRules;
    }

    public PackageWorkflowCoordinator(IPackageInventory inventory, ILocalIgnoreRuleStore ignoreRuleStore)
    {
        this.inventory = inventory;
        this.ignoreRuleStore = ignoreRuleStore;
    }

    public void RememberIgnoreRule(string relativePath, string reason)
    {
        if (ignoreRuleStore is null)
        {
            throw new InvalidOperationException("A local ignore-rule store is required before remembering a rule.");
        }

        var updatedRules = ignoreRuleStore.Load()
            .Where(rule => rule.Kind != LocalIgnoreRuleKind.RelativePath
                           || !rule.Pattern.Equals(relativePath, StringComparison.OrdinalIgnoreCase))
            .Append(new LocalIgnoreRule(LocalIgnoreRuleKind.RelativePath, relativePath, reason, DateTimeOffset.UtcNow))
            .ToArray();
        ignoreRuleStore.Save(updatedRules);
    }

    public PackageWorkflowSnapshot Prepare(string sourceRoot, string outputParent)
    {
        var authorization = PackageRootAuthorizer.Authorize(sourceRoot, outputParent);

        if (!authorization.IsAuthorized)
        {
            var authorizationIssues = authorization.Issues
                .Select(ruleId => new PreflightIssue(
                    RuleId: ruleId,
                    Severity: PreflightSeverity.Blocked,
                    RelativePath: ".",
                    Explanation: "The selected local source and output roots are not authorized for this package project."))
                .ToArray();

            return new PackageWorkflowSnapshot(
                IsAuthorized: false,
                SourceRoot: authorization.SourceRoot,
                OutputParent: authorization.OutputParent,
                Entries: Array.Empty<Domain.Packages.PackageInventoryEntry>(),
                Issues: authorizationIssues);
        }

        var entries = LocalIgnoreRuleApplier.Apply(
            inventory.Index(authorization.SourceRoot),
            ignoreRuleStore?.Load() ?? fixedIgnoreRules ?? Array.Empty<LocalIgnoreRule>());
        var issues = DigitalAssetPackPreflight.Run(entries);

        return new PackageWorkflowSnapshot(
            IsAuthorized: true,
            SourceRoot: authorization.SourceRoot,
            OutputParent: authorization.OutputParent,
            Entries: entries,
            Issues: issues);
    }
}
