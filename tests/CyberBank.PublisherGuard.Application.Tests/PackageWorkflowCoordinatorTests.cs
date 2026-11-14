using CyberBank.PublisherGuard.Application.Packages;
using CyberBank.PublisherGuard.Domain.Packages;

namespace CyberBank.PublisherGuard.Application.Tests;

public sealed class PackageWorkflowCoordinatorTests
{
    [Fact]
    public void Prepare_authorizes_roots_indexes_entries_and_runs_preflight()
    {
        using var workspace = new TemporaryWorkspace();
        var sourceRoot = workspace.CreateDirectory("asset-pack");
        var outputParent = workspace.CreateDirectory("deliveries");
        var inventory = new StubInventory(
        [
            new PackageInventoryEntry("README.md", PackageFileType.Text, 10, "AB", PackageInclusionState.Included, null)
        ]);
        var coordinator = new PackageWorkflowCoordinator(inventory);

        var snapshot = coordinator.Prepare(sourceRoot, outputParent);

        Assert.True(snapshot.IsAuthorized);
        Assert.Equal(Path.GetFullPath(sourceRoot), snapshot.SourceRoot);
        Assert.Single(snapshot.Entries);
        Assert.Empty(snapshot.Issues);
        Assert.Equal(1, inventory.CallCount);
    }

    [Fact]
    public void Prepare_applies_local_ignore_rules_before_running_preflight()
    {
        using var workspace = new TemporaryWorkspace();
        var sourceRoot = workspace.CreateDirectory("asset-pack");
        var outputParent = workspace.CreateDirectory("deliveries");
        var coordinator = new PackageWorkflowCoordinator(
            new StubInventory(
            [
                new PackageInventoryEntry("README.md", PackageFileType.Text, 8, "AA", PackageInclusionState.Included, null),
                new PackageInventoryEntry("draft.tmp", PackageFileType.Text, 8, "BB", PackageInclusionState.Included, null, ["absolute-local-path"])
            ]),
            [new LocalIgnoreRule(LocalIgnoreRuleKind.FileNamePattern, "*.tmp", "Creator ignores temporary drafts.", DateTimeOffset.UtcNow)]);

        var snapshot = coordinator.Prepare(sourceRoot, outputParent);

        Assert.Empty(snapshot.Issues);
        var ignored = snapshot.Entries.Single(entry => entry.RelativePath == "draft.tmp");
        Assert.Equal(PackageInclusionState.Excluded, ignored.InclusionState);
        Assert.Equal("Creator ignores temporary drafts.", ignored.Reason);
    }

    [Fact]
    public void Remember_ignore_rule_persists_relative_path_and_applies_it_to_the_next_local_prepare()
    {
        using var workspace = new TemporaryWorkspace();
        var sourceRoot = workspace.CreateDirectory("asset-pack");
        var outputParent = workspace.CreateDirectory("deliveries");
        var store = new StubIgnoreRuleStore();
        var coordinator = new PackageWorkflowCoordinator(
            new StubInventory(
            [
                new PackageInventoryEntry("README.md", PackageFileType.Text, 8, "AA", PackageInclusionState.Included, null),
                new PackageInventoryEntry("draft.tmp", PackageFileType.Text, 8, "BB", PackageInclusionState.Included, null, ["absolute-local-path"])
            ]),
            store);

        coordinator.RememberIgnoreRule("draft.tmp", "Creator keeps this draft out of future deliveries.");
        var snapshot = coordinator.Prepare(sourceRoot, outputParent);

        var stored = Assert.Single(store.Rules);
        Assert.Equal(LocalIgnoreRuleKind.RelativePath, stored.Kind);
        Assert.Equal("draft.tmp", stored.Pattern);
        Assert.Empty(snapshot.Issues);
    }

    private sealed class StubInventory(IReadOnlyList<PackageInventoryEntry> entries) : IPackageInventory
    {
        public int CallCount { get; private set; }

        public IReadOnlyList<PackageInventoryEntry> Index(string sourceRoot)
        {
            CallCount++;
            return entries;
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
        private readonly string root = Path.Combine(Path.GetTempPath(), "CyberBank.PublisherGuard.Tests", Guid.NewGuid().ToString("N"));

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
