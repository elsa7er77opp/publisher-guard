using CyberBank.PublisherGuard.Application.Packages;

namespace CyberBank.PublisherGuard.Application.Tests;

public sealed class PackageRootAuthorizerTests
{
    [Fact]
    public void Authorize_allows_a_distinct_existing_output_parent()
    {
        using var workspace = new TemporaryWorkspace();
        var sourceRoot = workspace.CreateDirectory("asset-pack");
        var outputParent = workspace.CreateDirectory("deliveries");

        var result = PackageRootAuthorizer.Authorize(sourceRoot, outputParent);

        Assert.True(result.IsAuthorized);
        Assert.Equal(Path.GetFullPath(sourceRoot), result.SourceRoot);
        Assert.Equal(Path.GetFullPath(outputParent), result.OutputParent);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Authorize_rejects_an_output_parent_inside_the_source_root()
    {
        using var workspace = new TemporaryWorkspace();
        var sourceRoot = workspace.CreateDirectory("asset-pack");
        var outputParent = workspace.CreateDirectory("asset-pack/deliveries");

        var result = PackageRootAuthorizer.Authorize(sourceRoot, outputParent);

        Assert.False(result.IsAuthorized);
        Assert.Contains("PG-ROOT-002", result.Issues);
    }

    [Fact]
    public void Authorize_rejects_a_source_root_that_does_not_exist()
    {
        using var workspace = new TemporaryWorkspace();
        var missingSourceRoot = Path.Combine(Path.GetTempPath(), "CyberBank.PublisherGuard.Tests", Guid.NewGuid().ToString("N"));
        var outputParent = workspace.CreateDirectory("deliveries");

        var result = PackageRootAuthorizer.Authorize(missingSourceRoot, outputParent);

        Assert.False(result.IsAuthorized);
        Assert.Contains("PG-ROOT-001", result.Issues);
    }

    [Fact]
    public void Authorize_rejects_an_output_parent_that_does_not_exist()
    {
        using var workspace = new TemporaryWorkspace();
        var sourceRoot = workspace.CreateDirectory("asset-pack");
        var missingOutputParent = Path.Combine(Path.GetTempPath(), "CyberBank.PublisherGuard.Tests", Guid.NewGuid().ToString("N"));

        var result = PackageRootAuthorizer.Authorize(sourceRoot, missingOutputParent);

        Assert.False(result.IsAuthorized);
        Assert.Contains("PG-ROOT-001", result.Issues);
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
