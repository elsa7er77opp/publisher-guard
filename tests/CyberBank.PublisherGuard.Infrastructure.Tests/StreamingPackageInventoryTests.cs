using System.Security.Cryptography;
using System.Text;
using CyberBank.PublisherGuard.Application.Packages;
using CyberBank.PublisherGuard.Domain.Packages;
using CyberBank.PublisherGuard.Infrastructure.Packages;

namespace CyberBank.PublisherGuard.Infrastructure.Tests;

public sealed class StreamingPackageInventoryTests
{
    [Fact]
    public void Local_inventory_implements_the_application_inventory_contract()
    {
        using var workspace = new TemporaryWorkspace();
        var sourceRoot = workspace.CreateDirectory("asset-pack");
        File.WriteAllBytes(Path.Combine(sourceRoot, "README.md"), [0x01]);
        IPackageInventory inventory = new LocalPackageInventory();

        var entry = Assert.Single(inventory.Index(sourceRoot));

        Assert.Equal("README.md", entry.RelativePath);
    }

    [Fact]
    public void Index_marks_executable_payloads_as_excluded_without_execution()
    {
        using var workspace = new TemporaryWorkspace();
        var sourceRoot = workspace.CreateDirectory("asset-pack");
        File.WriteAllBytes(Path.Combine(sourceRoot, "release-helper.exe"), [0x4D, 0x5A]);

        var entry = Assert.Single(StreamingPackageInventory.Index(sourceRoot));

        Assert.Equal(PackageFileType.Executable, entry.FileType);
        Assert.Equal(PackageInclusionState.Excluded, entry.InclusionState);
        Assert.Equal("PG-CONTENT-050", entry.Reason);
    }

    [Fact]
    public void Index_marks_text_metadata_that_contains_an_absolute_windows_path()
    {
        using var workspace = new TemporaryWorkspace();
        var sourceRoot = workspace.CreateDirectory("asset-pack");
        var filePath = Path.Combine(sourceRoot, "template.json");
        File.WriteAllBytes(filePath, Encoding.UTF8.GetBytes("{\"source\":\"C:\\Users\\creator\\Drafts\\bundle\"}"));

        var entry = Assert.Single(StreamingPackageInventory.Index(sourceRoot));

        Assert.Contains("absolute-local-path", entry.MetadataSignals!);
    }

    [Fact]
    public void Index_excludes_the_windows_thumbnail_artifact_with_a_reviewable_reason()
    {
        using var workspace = new TemporaryWorkspace();
        var sourceRoot = workspace.CreateDirectory("asset-pack");
        File.WriteAllBytes(Path.Combine(sourceRoot, "Thumbs.db"), [0x01]);

        var entry = Assert.Single(StreamingPackageInventory.Index(sourceRoot));

        Assert.Equal(PackageInclusionState.Excluded, entry.InclusionState);
        Assert.Equal("PG-NAME-020", entry.Reason);
    }

    [Fact]
    public void Index_returns_relative_metadata_and_sha256_without_file_contents()
    {
        using var workspace = new TemporaryWorkspace();
        var sourceRoot = workspace.CreateDirectory("asset-pack");
        var relativePath = Path.Combine("docs", "README.txt");
        var absolutePath = Path.Combine(sourceRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        var contents = Encoding.UTF8.GetBytes("Publisher Guard local evidence");
        File.WriteAllBytes(absolutePath, contents);

        var entries = StreamingPackageInventory.Index(sourceRoot);
        var entry = Assert.Single(entries);

        Assert.Equal(Path.Combine("docs", "README.txt"), entry.RelativePath);
        Assert.Equal(PackageFileType.Text, entry.FileType);
        Assert.Equal(contents.LongLength, entry.ByteLength);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(contents)), entry.Sha256);
        Assert.Equal(PackageInclusionState.Included, entry.InclusionState);
        Assert.Null(entry.Reason);
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
