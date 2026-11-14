using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CyberBank.PublisherGuard.Application.Delivery;
using CyberBank.PublisherGuard.Application.Packages;
using CyberBank.PublisherGuard.Application.Preflight;
using CyberBank.PublisherGuard.Domain.Evidence;
using CyberBank.PublisherGuard.Domain.Packages;
using CyberBank.PublisherGuard.Domain.Preflight;

namespace CyberBank.PublisherGuard.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly PackageWorkflowCoordinator? workflowCoordinator;
    private readonly CleanPackageBuildQueue? cleanPackageBuildQueue;
    private PackageWorkflowSnapshot? currentSnapshot;
    private readonly List<DeliveryDecision> deliveryDecisions = [];

    public MainViewModel()
    {
    }

    public MainViewModel(PackageWorkflowCoordinator workflowCoordinator)
    {
        this.workflowCoordinator = workflowCoordinator;
    }

    public MainViewModel(PackageWorkflowCoordinator workflowCoordinator, ICleanPackageBuilder cleanPackageBuilder)
        : this(workflowCoordinator)
    {
        cleanPackageBuildQueue = new CleanPackageBuildQueue(cleanPackageBuilder);
    }

    [ObservableProperty]
    public partial string LocalEngineState { get; set; } = "NO PACKAGE AUTHORIZED";

    [ObservableProperty]
    public partial string SourceRootDisplay { get; set; } = "No package selected";

    [ObservableProperty]
    public partial string ActiveStage { get; set; } = "CHOOSE PACKAGE";

    [ObservableProperty]
    public partial string ReadinessHeadline { get; set; } = "Choose a package to check";

    [ObservableProperty]
    public partial string FriendlyState { get; set; } = "Choose a package to check";

    [ObservableProperty]
    public partial string QuickCheckSummary { get; set; } = "No package selected";

    [ObservableProperty]
    public partial string ChangeSummary { get; set; } = "No earlier check to compare";

    [ObservableProperty]
    public partial string PrimaryActionLabel { get; set; } = "Choose package";

    public PackagePreset ActivePreset { get; } = PackagePreset.StandardAssetPack;

    public string PresetName => ActivePreset.Name;

    public string PresetDescription => ActivePreset.Description;

    [ObservableProperty]
    public partial string DetailsHeadline { get; set; } = "Choose an item to view details";

    [ObservableProperty]
    public partial string DetailsExplanation { get; set; } = "Select a package item to see what Publisher Guard found and what it means for delivery.";

    [ObservableProperty]
    public partial string DetailsActionLabel { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDeliveryOutput))]
    public partial string DeliveryOutputRoot { get; set; } = "";

    public bool HasDeliveryOutput => !string.IsNullOrWhiteSpace(DeliveryOutputRoot);

    [ObservableProperty]
    public partial bool IsBuildActive { get; set; }

    public ObservableCollection<PackageEntryRow> PackageEntries { get; } = [];

    public ObservableCollection<IssueRow> Issues { get; } = [];

    public ObservableCollection<QueueRow> BuildQueue { get; } = [];

    public void Prepare(string sourceRoot, string outputParent)
    {
        if (workflowCoordinator is null)
        {
            throw new InvalidOperationException("A local workflow coordinator is required before preparing a package.");
        }

        Apply(workflowCoordinator.Prepare(sourceRoot, outputParent));
    }

    public bool TryBuildCleanDelivery(string deliveryName)
    {
        return TryBuildCleanDeliveryAsync(deliveryName, CancellationToken.None).GetAwaiter().GetResult();
    }

    public async Task<bool> TryBuildCleanDeliveryAsync(string deliveryName, CancellationToken cancellationToken)
    {
        if (cleanPackageBuildQueue is null || currentSnapshot is null || !currentSnapshot.IsAuthorized
            || currentSnapshot.Issues.Any())
        {
            return false;
        }

        var manifest = CleanPackageManifest.Create(PackageProfile.Default, currentSnapshot.Entries);
        IsBuildActive = true;
        BuildQueue.Add(new QueueRow("Clean delivery", "Running", deliveryName));

        try
        {
            var outcome = await cleanPackageBuildQueue.EnqueueAsync(
                new CleanPackageBuildCommand(
                    currentSnapshot.SourceRoot,
                    currentSnapshot.OutputParent,
                    deliveryName,
                    manifest,
                    deliveryDecisions),
                cancellationToken);
            if (!outcome.IsComplete)
            {
                return false;
            }

            BuildQueue.Add(new QueueRow("Clean delivery", "Complete", outcome.OutputRoot));
            LocalEngineState = "DELIVERY EVIDENCE READY";
            DeliveryOutputRoot = outcome.OutputRoot;
            ActiveStage = "DELIVERY COMPLETE";
            ReadinessHeadline = "Delivery evidence ready";
            FriendlyState = "Delivery ready";
            PrimaryActionLabel = "Open delivery folder";
            DetailsHeadline = "Clean delivery created";
            DetailsExplanation = "Your selected files, readable delivery report, manifest, SHA-256 evidence, and local activity record are ready in the delivery folder.";
            DetailsActionLabel = "Open delivery folder";
            return true;
        }
        catch (OperationCanceledException)
        {
            BuildQueue.Add(new QueueRow("Clean delivery", "Cancelled", deliveryName));
            LocalEngineState = "BUILD CANCELLED";
            return false;
        }
        finally
        {
            IsBuildActive = false;
        }
    }

    public void Exclude(string relativePath, string reason)
    {
        if (currentSnapshot is null)
        {
            return;
        }

        var decision = new DeliveryDecision(relativePath, PackageInclusionState.Excluded, reason, DateTimeOffset.UtcNow);
        deliveryDecisions.RemoveAll(existing => existing.RelativePath.Equals(relativePath, StringComparison.OrdinalIgnoreCase));
        deliveryDecisions.Add(decision);

        var updatedEntries = PackageInclusionDecisionService.Apply(currentSnapshot.Entries, decision);
        var updatedIssues = DigitalAssetPackPreflight.Run(updatedEntries);
        Apply(currentSnapshot with { Entries = updatedEntries, Issues = updatedIssues });
    }

    public void RememberIgnoreRule(string relativePath, string reason)
    {
        if (workflowCoordinator is null || currentSnapshot is null || !currentSnapshot.IsAuthorized)
        {
            return;
        }

        workflowCoordinator.RememberIgnoreRule(relativePath, reason);
        Apply(workflowCoordinator.Prepare(currentSnapshot.SourceRoot, currentSnapshot.OutputParent));
    }

    public void SelectIssue(IssueRow issue)
    {
        DetailsHeadline = $"{issue.RelativePath} needs attention";
        DetailsExplanation = issue.Summary;
        DetailsActionLabel = "Keep out of delivery";
    }

    public void Apply(PackageWorkflowSnapshot snapshot)
    {
        var previousSnapshot = currentSnapshot;
        if (currentSnapshot is null || !currentSnapshot.SourceRoot.Equals(snapshot.SourceRoot, StringComparison.OrdinalIgnoreCase))
        {
            deliveryDecisions.Clear();
        }

        if (previousSnapshot is not null
            && previousSnapshot.SourceRoot.Equals(snapshot.SourceRoot, StringComparison.OrdinalIgnoreCase))
        {
            var comparison = PackageInventoryComparer.Compare(previousSnapshot.Entries, snapshot.Entries);
            ChangeSummary = $"{comparison.Added.Count} added · {comparison.Removed.Count} removed · {comparison.Changed.Count} changed since last check";
        }
        else
        {
            ChangeSummary = "No earlier check to compare";
        }

        currentSnapshot = snapshot;

        var includedCount = snapshot.Entries.Count(entry => entry.InclusionState == PackageInclusionState.Included);
        var excludedCount = snapshot.Entries.Count(entry => entry.InclusionState == PackageInclusionState.Excluded);
        QuickCheckSummary = $"{includedCount} included · {excludedCount} kept out · {snapshot.Issues.Count} needs attention";

        foreach (var automaticallyExcludedEntry in snapshot.Entries.Where(entry =>
                     entry.InclusionState == PackageInclusionState.Excluded
                     && !string.IsNullOrWhiteSpace(entry.Reason)))
        {
            if (deliveryDecisions.All(decision => !decision.RelativePath.Equals(automaticallyExcludedEntry.RelativePath, StringComparison.OrdinalIgnoreCase)))
            {
                deliveryDecisions.Add(new DeliveryDecision(
                    automaticallyExcludedEntry.RelativePath,
                    PackageInclusionState.Excluded,
                    automaticallyExcludedEntry.Reason!,
                    DateTimeOffset.UtcNow));
            }
        }

        SourceRootDisplay = snapshot.SourceRoot;
        LocalEngineState = !snapshot.IsAuthorized
            ? "AUTHORIZATION REQUIRED"
            : snapshot.Issues.Any(issue => issue.Severity == PreflightSeverity.Blocked)
                ? "PREFLIGHT BLOCKED"
                : snapshot.Issues.Count > 0
                    ? "REVIEW REQUIRED"
                    : "PREFLIGHT READY";

        if (!snapshot.IsAuthorized)
        {
            ActiveStage = "CHOOSE PACKAGE";
            ReadinessHeadline = "Choose a package to check";
            FriendlyState = "Choose a package to check";
            PrimaryActionLabel = "Choose package";
        }
        else if (snapshot.Issues.Any())
        {
            ActiveStage = "RESOLVE ITEMS";
            ReadinessHeadline = "This package needs attention";
            FriendlyState = "Needs attention";
            PrimaryActionLabel = "Review items";
        }
        else
        {
            ActiveStage = "CREATE DELIVERY";
            ReadinessHeadline = "Ready to create delivery";
            FriendlyState = "Ready for delivery";
            PrimaryActionLabel = "Create clean delivery";
        }

        PackageEntries.Clear();
        foreach (var entry in snapshot.Entries)
        {
            PackageEntries.Add(new PackageEntryRow(
                DisplayName: Path.GetFileName(entry.RelativePath),
                RelativePath: entry.RelativePath,
                Type: entry.FileType.ToString(),
                Size: $"{entry.ByteLength:N0} bytes",
                State: entry.InclusionState.ToString(),
                Reason: entry.Reason));
        }

        Issues.Clear();
        foreach (var issue in snapshot.Issues)
        {
            Issues.Add(new IssueRow(
                Severity: issue.Severity.ToString(),
                RuleId: issue.RuleId,
                RelativePath: issue.RelativePath,
                Summary: issue.Explanation,
                State: issue.Severity == PreflightSeverity.Blocked ? "Blocked" : "Review"));
        }
    }
}

public sealed record PackageEntryRow(
    string DisplayName,
    string RelativePath,
    string Type,
    string Size,
    string State,
    string? Reason);

public sealed record IssueRow(
    string Severity,
    string RuleId,
    string RelativePath,
    string Summary,
    string State);

public sealed record QueueRow(
    string Operation,
    string Status,
    string Detail);
