using System.IO;
using CyberBank.PublisherGuard.ViewModels;
using CyberBank.PublisherGuard.Application.Delivery;
using CyberBank.PublisherGuard.Application.Packages;
using CyberBank.PublisherGuard.Domain.Evidence;
using CyberBank.PublisherGuard.Domain.Packages;

namespace CyberBank.PublisherGuard.Desktop.Tests;

public sealed class MainViewModelTests
{
    [Fact]
    public void Empty_workstation_exposes_local_state_and_no_fabricated_package_data()
    {
        var viewModel = new MainViewModel();

        Assert.Equal("NO PACKAGE AUTHORIZED", viewModel.LocalEngineState);
        Assert.Equal("Choose package", viewModel.PrimaryActionLabel);
        Assert.Equal("CHOOSE PACKAGE", viewModel.ActiveStage);
        Assert.Equal("Choose a package to check", viewModel.ReadinessHeadline);
        Assert.Equal("Choose a package to check", viewModel.FriendlyState);
        Assert.Equal("No package selected", viewModel.SourceRootDisplay);
        Assert.Empty(viewModel.PackageEntries);
        Assert.Empty(viewModel.Issues);
    }

    [Fact]
    public void Empty_workstation_exposes_the_plain_language_default_package_preset()
    {
        var viewModel = new MainViewModel();

        Assert.Equal("Standard Asset Pack", viewModel.PresetName);
        Assert.Equal("Checks a typical template or digital asset delivery.", viewModel.PresetDescription);
    }

    [Fact]
    public void Apply_projects_a_real_ready_workflow_snapshot_into_the_workstation()
    {
        var viewModel = new MainViewModel();
        var snapshot = new PackageWorkflowSnapshot(
            IsAuthorized: true,
            SourceRoot: @"C:\packages\landing-kit",
            OutputParent: @"C:\deliveries",
            Entries:
            [
                new PackageInventoryEntry("README.md", PackageFileType.Text, 128, "AB", PackageInclusionState.Included, null)
            ],
            Issues: Array.Empty<CyberBank.PublisherGuard.Domain.Preflight.PreflightIssue>());

        viewModel.Apply(snapshot);

        Assert.Equal("PREFLIGHT READY", viewModel.LocalEngineState);
        Assert.Equal(@"C:\packages\landing-kit", viewModel.SourceRootDisplay);
        Assert.Equal("CREATE DELIVERY", viewModel.ActiveStage);
        Assert.Equal("Ready to create delivery", viewModel.ReadinessHeadline);
        Assert.Equal("Create clean delivery", viewModel.PrimaryActionLabel);
        Assert.Equal("Ready for delivery", viewModel.FriendlyState);
        var entry = Assert.Single(viewModel.PackageEntries);
        Assert.Equal("README.md", entry.RelativePath);
    }

    [Fact]
    public void Apply_projects_a_plain_quick_check_summary_from_real_inventory_and_findings()
    {
        var viewModel = new MainViewModel();
        viewModel.Apply(new PackageWorkflowSnapshot(
            IsAuthorized: true,
            SourceRoot: @"C:\packages\landing-kit",
            OutputParent: @"C:\deliveries",
            Entries:
            [
                new PackageInventoryEntry("README.md", PackageFileType.Text, 128, "AB", PackageInclusionState.Included, null),
                new PackageInventoryEntry("Thumbs.db", PackageFileType.Binary, 4, "CD", PackageInclusionState.Excluded, "PG-NAME-020")
            ],
            Issues: [new CyberBank.PublisherGuard.Domain.Preflight.PreflightIssue(
                "PG-REQUIRED-010",
                CyberBank.PublisherGuard.Domain.Preflight.PreflightSeverity.ReviewRequired,
                "README.md",
                "A creator README is required for human review.")]));

        Assert.Equal("1 included · 1 kept out · 1 needs attention", viewModel.QuickCheckSummary);
    }

    [Fact]
    public void Apply_a_second_package_check_projects_plain_language_hash_changes()
    {
        var viewModel = new MainViewModel();
        viewModel.Apply(new PackageWorkflowSnapshot(
            IsAuthorized: true,
            SourceRoot: @"C:\packages\landing-kit",
            OutputParent: @"C:\deliveries",
            Entries:
            [
                new PackageInventoryEntry("README.md", PackageFileType.Text, 4, "AAAA", PackageInclusionState.Included, null),
                new PackageInventoryEntry("old-preview.png", PackageFileType.Binary, 8, "BBBB", PackageInclusionState.Included, null)
            ],
            Issues: Array.Empty<CyberBank.PublisherGuard.Domain.Preflight.PreflightIssue>()));

        viewModel.Apply(new PackageWorkflowSnapshot(
            IsAuthorized: true,
            SourceRoot: @"C:\packages\landing-kit",
            OutputParent: @"C:\deliveries",
            Entries:
            [
                new PackageInventoryEntry("README.md", PackageFileType.Text, 5, "CCCC", PackageInclusionState.Included, null),
                new PackageInventoryEntry("new-preview.png", PackageFileType.Binary, 8, "DDDD", PackageInclusionState.Included, null)
            ],
            Issues: Array.Empty<CyberBank.PublisherGuard.Domain.Preflight.PreflightIssue>()));

        Assert.Equal("1 added · 1 removed · 1 changed since last check", viewModel.ChangeSummary);
    }

    [Fact]
    public void Prepare_runs_the_local_workflow_and_projects_the_result()
    {
        using var workspace = new TemporaryWorkspace();
        var sourceRoot = workspace.CreateDirectory("asset-pack");
        var outputParent = workspace.CreateDirectory("deliveries");
        var coordinator = new PackageWorkflowCoordinator(new StubInventory(
        [
            new PackageInventoryEntry("README.md", PackageFileType.Text, 8, "AC", PackageInclusionState.Included, null)
        ]));
        var viewModel = new MainViewModel(coordinator);

        viewModel.Prepare(sourceRoot, outputParent);

        Assert.Equal("PREFLIGHT READY", viewModel.LocalEngineState);
        Assert.Single(viewModel.PackageEntries);
    }

    [Fact]
    public void Build_clean_delivery_writes_an_evidence_queue_entry_after_a_ready_preflight()
    {
        using var workspace = new TemporaryWorkspace();
        var sourceRoot = workspace.CreateDirectory("asset-pack");
        var outputParent = workspace.CreateDirectory("deliveries");
        var coordinator = new PackageWorkflowCoordinator(new StubInventory(
        [
            new PackageInventoryEntry("README.md", PackageFileType.Text, 8, "AC", PackageInclusionState.Included, null)
        ]));
        var builder = new StubCleanPackageBuilder();
        var viewModel = new MainViewModel(coordinator, builder);
        viewModel.Prepare(sourceRoot, outputParent);

        var built = viewModel.TryBuildCleanDelivery("asset-pack-clean");

        Assert.True(built);
        Assert.Equal("DELIVERY EVIDENCE READY", viewModel.LocalEngineState);
        Assert.Equal("asset-pack-clean", builder.LastDeliveryName);
        Assert.Contains(viewModel.BuildQueue, row => row.Status == "Running");
        Assert.Contains(viewModel.BuildQueue, row => row.Status == "Complete");
    }

    [Fact]
    public void Build_clean_delivery_projects_a_completed_local_delivery_for_the_workstation()
    {
        using var workspace = new TemporaryWorkspace();
        var sourceRoot = workspace.CreateDirectory("asset-pack");
        var outputParent = workspace.CreateDirectory("deliveries");
        var coordinator = new PackageWorkflowCoordinator(new StubInventory(
        [
            new PackageInventoryEntry("README.md", PackageFileType.Text, 8, "AC", PackageInclusionState.Included, null)
        ]));
        var builder = new StubCleanPackageBuilder();
        var viewModel = new MainViewModel(coordinator, builder);
        viewModel.Prepare(sourceRoot, outputParent);

        var built = viewModel.TryBuildCleanDelivery("asset-pack-clean");

        Assert.True(built);
        Assert.Equal("DELIVERY COMPLETE", viewModel.ActiveStage);
        Assert.Equal("Delivery evidence ready", viewModel.ReadinessHeadline);
        Assert.Equal("Open delivery folder", viewModel.PrimaryActionLabel);
        Assert.Equal(Path.Combine(outputParent, "asset-pack-clean"), viewModel.DeliveryOutputRoot);
        Assert.True(viewModel.HasDeliveryOutput);
    }

    [Fact]
    public void Build_clean_delivery_is_refused_until_review_required_preflight_is_resolved()
    {
        var builder = new StubCleanPackageBuilder();
        var viewModel = new MainViewModel(new PackageWorkflowCoordinator(new StubInventory([])), builder);
        viewModel.Apply(new PackageWorkflowSnapshot(
            IsAuthorized: true,
            SourceRoot: @"C:\packages\landing-kit",
            OutputParent: @"C:\deliveries",
            Entries: [new PackageInventoryEntry("README.md", PackageFileType.Text, 8, "AC", PackageInclusionState.Included, null)],
            Issues: [new CyberBank.PublisherGuard.Domain.Preflight.PreflightIssue(
                "PG-REQUIRED-010",
                CyberBank.PublisherGuard.Domain.Preflight.PreflightSeverity.ReviewRequired,
                "README.md",
                "A creator README is required for human review.")]));

        var built = viewModel.TryBuildCleanDelivery("asset-pack-clean");

        Assert.False(built);
        Assert.Empty(viewModel.BuildQueue);
        Assert.Null(builder.LastDeliveryName);
    }

    [Fact]
    public void Apply_projects_open_findings_as_plain_language_resolve_items_state()
    {
        var viewModel = new MainViewModel();
        viewModel.Apply(new PackageWorkflowSnapshot(
            IsAuthorized: true,
            SourceRoot: @"C:\packages\landing-kit",
            OutputParent: @"C:\deliveries",
            Entries: [new PackageInventoryEntry("README.md", PackageFileType.Text, 8, "AC", PackageInclusionState.Included, null)],
            Issues: [new CyberBank.PublisherGuard.Domain.Preflight.PreflightIssue(
                "PG-REQUIRED-010",
                CyberBank.PublisherGuard.Domain.Preflight.PreflightSeverity.ReviewRequired,
                "README.md",
                "A creator README is required for human review.")]));

        Assert.Equal("RESOLVE ITEMS", viewModel.ActiveStage);
        Assert.Equal("This package needs attention", viewModel.ReadinessHeadline);
        Assert.Equal("Review items", viewModel.PrimaryActionLabel);
        Assert.Equal("Needs attention", viewModel.FriendlyState);
    }

    [Fact]
    public void SelectIssue_projects_plain_language_delivery_details()
    {
        var viewModel = new MainViewModel();
        viewModel.Apply(new PackageWorkflowSnapshot(
            IsAuthorized: true,
            SourceRoot: @"C:\packages\landing-kit",
            OutputParent: @"C:\deliveries",
            Entries: [new PackageInventoryEntry("README.md", PackageFileType.Text, 8, "AC", PackageInclusionState.Included, null)],
            Issues: [new CyberBank.PublisherGuard.Domain.Preflight.PreflightIssue(
                "PG-REQUIRED-010",
                CyberBank.PublisherGuard.Domain.Preflight.PreflightSeverity.ReviewRequired,
                "README.md",
                "A creator README is required for human review.")]));

        viewModel.SelectIssue(Assert.Single(viewModel.Issues));

        Assert.Equal("README.md needs attention", viewModel.DetailsHeadline);
        Assert.Equal("A creator README is required for human review.", viewModel.DetailsExplanation);
        Assert.Equal("Keep out of delivery", viewModel.DetailsActionLabel);
    }

    [Fact]
    public void Exclude_rechecks_preflight_and_unblocks_a_metadata_review_when_the_file_is_removed()
    {
        var viewModel = new MainViewModel();
        viewModel.Apply(new PackageWorkflowSnapshot(
            IsAuthorized: true,
            SourceRoot: @"C:\packages\landing-kit",
            OutputParent: @"C:\deliveries",
            Entries: [new PackageInventoryEntry("template.json", PackageFileType.Text, 8, "AC", PackageInclusionState.Included, null, ["absolute-local-path"]), new PackageInventoryEntry("README.md", PackageFileType.Text, 8, "AB", PackageInclusionState.Included, null)],
            Issues: [new CyberBank.PublisherGuard.Domain.Preflight.PreflightIssue(
                "PG-METADATA-040",
                CyberBank.PublisherGuard.Domain.Preflight.PreflightSeverity.ReviewRequired,
                "template.json",
                "Text metadata appears to contain an absolute local path and needs human review.")]));

        viewModel.Exclude("template.json", "Creator excluded the template from the delivery package.");

        Assert.Equal("PREFLIGHT READY", viewModel.LocalEngineState);
        Assert.Empty(viewModel.Issues);
        Assert.Equal(PackageInclusionState.Excluded.ToString(), viewModel.PackageEntries.Single(entry => entry.RelativePath == "template.json").State);
    }

    [Fact]
    public void Remember_ignore_rule_persists_the_choice_and_rechecks_the_local_package()
    {
        using var workspace = new TemporaryWorkspace();
        var sourceRoot = workspace.CreateDirectory("asset-pack");
        var outputParent = workspace.CreateDirectory("deliveries");
        var store = new StubIgnoreRuleStore();
        var coordinator = new PackageWorkflowCoordinator(
            new StubInventory(
            [
                new PackageInventoryEntry("README.md", PackageFileType.Text, 8, "AB", PackageInclusionState.Included, null),
                new PackageInventoryEntry("template.json", PackageFileType.Text, 8, "AC", PackageInclusionState.Included, null, ["absolute-local-path"])
            ]),
            store);
        var viewModel = new MainViewModel(coordinator);
        viewModel.Prepare(sourceRoot, outputParent);

        viewModel.RememberIgnoreRule("template.json", "Creator keeps this template out of future deliveries.");

        Assert.Single(store.Rules);
        Assert.Empty(viewModel.Issues);
        Assert.Equal(PackageInclusionState.Excluded.ToString(), viewModel.PackageEntries.Single(entry => entry.RelativePath == "template.json").State);
    }

    [Fact]
    public void Build_clean_delivery_passes_exclusion_decisions_to_the_evidence_builder()
    {
        var builder = new StubCleanPackageBuilder();
        var viewModel = new MainViewModel(new PackageWorkflowCoordinator(new StubInventory([])), builder);
        viewModel.Apply(new PackageWorkflowSnapshot(
            IsAuthorized: true,
            SourceRoot: @"C:\packages\landing-kit",
            OutputParent: @"C:\deliveries",
            Entries: [new PackageInventoryEntry("template.json", PackageFileType.Text, 8, "AC", PackageInclusionState.Included, null, ["absolute-local-path"]), new PackageInventoryEntry("README.md", PackageFileType.Text, 8, "AB", PackageInclusionState.Included, null)],
            Issues: [new CyberBank.PublisherGuard.Domain.Preflight.PreflightIssue(
                "PG-METADATA-040",
                CyberBank.PublisherGuard.Domain.Preflight.PreflightSeverity.ReviewRequired,
                "template.json",
                "Text metadata appears to contain an absolute local path and needs human review.")]));
        viewModel.Exclude("template.json", "Creator excluded the template from delivery.");

        var built = viewModel.TryBuildCleanDelivery("landing-kit-clean");

        Assert.True(built);
        var decision = Assert.Single(builder.LastDecisions);
        Assert.Equal("template.json", decision.RelativePath);
        Assert.Equal("Creator excluded the template from delivery.", decision.Reason);
    }

    [Fact]
    public void Build_clean_delivery_records_automatic_exclusions_in_evidence()
    {
        var builder = new StubCleanPackageBuilder();
        var viewModel = new MainViewModel(new PackageWorkflowCoordinator(new StubInventory([])), builder);
        viewModel.Apply(new PackageWorkflowSnapshot(
            IsAuthorized: true,
            SourceRoot: @"C:\packages\landing-kit",
            OutputParent: @"C:\deliveries",
            Entries:
            [
                new PackageInventoryEntry("README.md", PackageFileType.Text, 8, "AC", PackageInclusionState.Included, null),
                new PackageInventoryEntry("Thumbs.db", PackageFileType.Binary, 2, "BD", PackageInclusionState.Excluded, "PG-NAME-020")
            ],
            Issues: Array.Empty<CyberBank.PublisherGuard.Domain.Preflight.PreflightIssue>()));

        var built = viewModel.TryBuildCleanDelivery("landing-kit-clean");

        Assert.True(built);
        var decision = Assert.Single(builder.LastDecisions);
        Assert.Equal("Thumbs.db", decision.RelativePath);
        Assert.Equal("PG-NAME-020", decision.Reason);
    }

    private sealed class StubInventory(IReadOnlyList<PackageInventoryEntry> entries) : IPackageInventory
    {
        public IReadOnlyList<PackageInventoryEntry> Index(string sourceRoot) => entries;
    }

    private sealed class StubCleanPackageBuilder : ICleanPackageBuilder
    {
        public string? LastDeliveryName { get; private set; }
        public IReadOnlyList<DeliveryDecision> LastDecisions { get; private set; } = Array.Empty<DeliveryDecision>();

        public CleanPackageBuildOutcome Build(
            string sourceRoot,
            string outputParent,
            string deliveryName,
            CleanPackageManifest manifest,
            IReadOnlyList<DeliveryDecision> decisions,
            CancellationToken cancellationToken = default)
        {
            LastDeliveryName = deliveryName;
            LastDecisions = decisions;
            return new CleanPackageBuildOutcome(true, Path.Combine(outputParent, deliveryName));
        }
    }

    private sealed class StubIgnoreRuleStore : ILocalIgnoreRuleStore
    {
        public List<LocalIgnoreRule> Rules { get; } = [];

        public IReadOnlyList<LocalIgnoreRule> Load() => Rules;

        public void Save(IReadOnlyList<LocalIgnoreRule> rules)
        {
            Rules.Clear();
            Rules.AddRange(rules);
        }
    }

    private sealed class TemporaryWorkspace : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), "CyberBank.PublisherGuard.Desktop.Tests", Guid.NewGuid().ToString("N"));

        public string CreateDirectory(string relativePath)
        {
            var path = Path.Combine(root, relativePath);
            Directory.CreateDirectory(path);
            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
