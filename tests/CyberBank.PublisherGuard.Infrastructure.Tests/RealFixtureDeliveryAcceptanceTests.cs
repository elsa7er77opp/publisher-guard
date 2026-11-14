using System.Text;
using CyberBank.PublisherGuard.Application.Packages;
using CyberBank.PublisherGuard.Application.Preflight;
using CyberBank.PublisherGuard.Domain.Evidence;
using CyberBank.PublisherGuard.Domain.Packages;
using CyberBank.PublisherGuard.Infrastructure.Delivery;
using CyberBank.PublisherGuard.Infrastructure.Packages;

namespace CyberBank.PublisherGuard.Infrastructure.Tests;

public sealed class RealFixtureDeliveryAcceptanceTests
{
    [Fact]
    public void Local_fixture_can_be_reviewed_resolved_and_delivered_with_evidence()
    {
        using var workspace = new TemporaryWorkspace();
        var sourceRoot = workspace.CreateDirectory("creator-asset-pack");
        var outputParent = workspace.CreateDirectory("deliveries");
        File.WriteAllText(Path.Combine(sourceRoot, "README.md"), "Creator delivery instructions", Encoding.UTF8);
        File.WriteAllText(Path.Combine(sourceRoot, "template.json"), "{\"source\":\"C:\\Users\\creator\\Drafts\\bundle\"}", Encoding.UTF8);
        File.WriteAllBytes(Path.Combine(sourceRoot, "Thumbs.db"), [0x01]);

        var prepared = new PackageWorkflowCoordinator(new LocalPackageInventory()).Prepare(sourceRoot, outputParent);

        Assert.True(prepared.IsAuthorized);
        Assert.Contains(prepared.Issues, issue => issue.RuleId == "PG-METADATA-040" && issue.RelativePath == "template.json");
        Assert.Contains(prepared.Entries, entry => entry.RelativePath == "Thumbs.db" && entry.InclusionState == PackageInclusionState.Excluded);

        var decision = new DeliveryDecision(
            "template.json",
            PackageInclusionState.Excluded,
            "Creator removed a machine-specific template reference.",
            DateTimeOffset.UtcNow);
        var resolvedEntries = PackageInclusionDecisionService.Apply(prepared.Entries, decision);

        Assert.Empty(DigitalAssetPackPreflight.Run(resolvedEntries));

        var result = AtomicCleanPackageBuilder.Build(new CleanPackageBuildRequest(
            sourceRoot,
            outputParent,
            "creator-asset-pack-clean",
            CleanPackageManifest.Create(PackageProfile.Default, resolvedEntries),
            [decision]));

        Assert.True(result.IsComplete);
        Assert.True(File.Exists(Path.Combine(result.OutputRoot, "README.md")));
        Assert.False(File.Exists(Path.Combine(result.OutputRoot, "template.json")));
        Assert.False(File.Exists(Path.Combine(result.OutputRoot, "Thumbs.db")));

        var manifest = File.ReadAllText(Path.Combine(result.OutputRoot, "publisher-guard.manifest.json"));
        var audit = File.ReadAllText(Path.Combine(result.OutputRoot, "publisher-guard.audit.json"));
        Assert.Contains("README.md", manifest, StringComparison.Ordinal);
        Assert.Contains("Sha256", manifest, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("template.json", audit, StringComparison.Ordinal);
        Assert.Contains("Creator removed a machine-specific template reference.", audit, StringComparison.Ordinal);
        Assert.DoesNotContain(sourceRoot, manifest, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(sourceRoot, audit, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class TemporaryWorkspace : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), "CyberBank.PublisherGuard.Acceptance", Guid.NewGuid().ToString("N"));

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
