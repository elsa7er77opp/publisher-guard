using System.Text;
using CyberBank.PublisherGuard.Domain.Evidence;
using CyberBank.PublisherGuard.Domain.Packages;
using CyberBank.PublisherGuard.Infrastructure.Delivery;

namespace CyberBank.PublisherGuard.Infrastructure.Tests;

public sealed class AtomicCleanPackageBuilderTests
{
    [Fact]
    public void Build_when_cancelled_before_copy_leaves_no_output_or_staging_directory()
    {
        using var workspace = new TemporaryWorkspace();
        var sourceRoot = workspace.CreateDirectory("asset-pack");
        var outputParent = workspace.CreateDirectory("deliveries");
        File.WriteAllBytes(Path.Combine(sourceRoot, "README.md"), Encoding.UTF8.GetBytes("Creator instructions"));
        var request = new CleanPackageBuildRequest(
            SourceRoot: sourceRoot,
            OutputParent: outputParent,
            DeliveryName: "landing-kit-clean",
            Manifest: CleanPackageManifest.Create(PackageProfile.Default,
            [
                new PackageInventoryEntry("README.md", PackageFileType.Text, 20, "AA", PackageInclusionState.Included, null)
            ]));
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        Assert.Throws<OperationCanceledException>(() => AtomicCleanPackageBuilder.Build(request, cancellationSource.Token));

        Assert.False(Directory.Exists(Path.Combine(outputParent, "landing-kit-clean")));
        Assert.Empty(Directory.GetDirectories(outputParent, ".publisher-guard-staging-*"));
    }

    [Fact]
    public void Build_copies_only_included_entries_and_writes_local_manifest_evidence()
    {
        using var workspace = new TemporaryWorkspace();
        var sourceRoot = workspace.CreateDirectory("asset-pack");
        var outputParent = workspace.CreateDirectory("deliveries");
        File.WriteAllBytes(Path.Combine(sourceRoot, "README.md"), Encoding.UTF8.GetBytes("Creator instructions"));
        File.WriteAllBytes(Path.Combine(sourceRoot, "Thumbs.db"), [0x01]);

        var entries = new[]
        {
            new PackageInventoryEntry("README.md", PackageFileType.Text, 20, "AA", PackageInclusionState.Included, null),
            new PackageInventoryEntry("Thumbs.db", PackageFileType.Binary, 1, "BB", PackageInclusionState.Excluded, "PG-NAME-020")
        };
        var result = AtomicCleanPackageBuilder.Build(new CleanPackageBuildRequest(
            SourceRoot: sourceRoot,
            OutputParent: outputParent,
            DeliveryName: "landing-kit-clean",
            Manifest: CleanPackageManifest.Create(PackageProfile.Default, entries),
            Decisions:
            [
                new DeliveryDecision("Thumbs.db", PackageInclusionState.Excluded, "Creator excluded the Windows thumbnail artifact.", DateTimeOffset.UtcNow)
            ]));

        Assert.True(result.IsComplete);
        Assert.True(File.Exists(Path.Combine(result.OutputRoot, "README.md")));
        Assert.False(File.Exists(Path.Combine(result.OutputRoot, "Thumbs.db")));

        var manifestJson = File.ReadAllText(Path.Combine(result.OutputRoot, "publisher-guard.manifest.json"));
        Assert.Contains("README.md", manifestJson, StringComparison.Ordinal);
        Assert.DoesNotContain(sourceRoot, manifestJson, StringComparison.OrdinalIgnoreCase);

        var auditJson = File.ReadAllText(Path.Combine(result.OutputRoot, "publisher-guard.audit.json"));
        Assert.Contains("clean-delivery-complete", auditJson, StringComparison.Ordinal);
        Assert.Contains("Creator excluded the Windows thumbnail artifact.", auditJson, StringComparison.Ordinal);
        Assert.Contains("Thumbs.db", auditJson, StringComparison.Ordinal);
        Assert.DoesNotContain(sourceRoot, auditJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_writes_a_readable_local_delivery_report_without_absolute_source_path()
    {
        using var workspace = new TemporaryWorkspace();
        var sourceRoot = workspace.CreateDirectory("asset-pack");
        var outputParent = workspace.CreateDirectory("deliveries");
        File.WriteAllBytes(Path.Combine(sourceRoot, "README.md"), Encoding.UTF8.GetBytes("Creator instructions"));
        File.WriteAllBytes(Path.Combine(sourceRoot, "Thumbs.db"), [0x01]);

        var result = AtomicCleanPackageBuilder.Build(new CleanPackageBuildRequest(
            SourceRoot: sourceRoot,
            OutputParent: outputParent,
            DeliveryName: "landing-kit-clean",
            Manifest: CleanPackageManifest.Create(PackageProfile.Default,
            [
                new PackageInventoryEntry("README.md", PackageFileType.Text, 20, "AA", PackageInclusionState.Included, null),
                new PackageInventoryEntry("Thumbs.db", PackageFileType.Binary, 1, "BB", PackageInclusionState.Excluded, "PG-NAME-020")
            ]),
            Decisions:
            [
                new DeliveryDecision("Thumbs.db", PackageInclusionState.Excluded, "Windows thumbnail artifact kept out.", DateTimeOffset.UtcNow)
            ]));

        var report = File.ReadAllText(Path.Combine(result.OutputRoot, "delivery-report.md"));

        Assert.Contains("# Delivery report", report, StringComparison.Ordinal);
        Assert.Contains("1 included file", report, StringComparison.Ordinal);
        Assert.Contains("1 kept-out file", report, StringComparison.Ordinal);
        Assert.Contains("Thumbs.db", report, StringComparison.Ordinal);
        Assert.DoesNotContain(sourceRoot, report, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class TemporaryWorkspace : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), "CyberBank.PublisherGuard.Infrastructure.Tests", Guid.NewGuid().ToString("N"));

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
