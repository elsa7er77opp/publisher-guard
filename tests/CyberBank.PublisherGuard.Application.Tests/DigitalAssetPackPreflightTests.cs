using CyberBank.PublisherGuard.Application.Preflight;
using CyberBank.PublisherGuard.Domain.Packages;
using CyberBank.PublisherGuard.Domain.Preflight;

namespace CyberBank.PublisherGuard.Application.Tests;

public sealed class DigitalAssetPackPreflightTests
{
    [Fact]
    public void Run_blocks_an_empty_package()
    {
        var issues = DigitalAssetPackPreflight.Run(Array.Empty<PackageInventoryEntry>());
        var issue = Assert.Single(issues);

        Assert.Equal("PG-PACKAGE-001", issue.RuleId);
        Assert.Equal(PreflightSeverity.Blocked, issue.Severity);
        Assert.Equal(".", issue.RelativePath);
    }

    [Fact]
    public void Run_marks_a_package_without_a_readme_for_review()
    {
        var entries = new[]
        {
            new PackageInventoryEntry(
                RelativePath: "template.json",
                FileType: PackageFileType.Text,
                ByteLength: 2,
                Sha256: "AB",
                InclusionState: PackageInclusionState.Included,
                Reason: null)
        };

        var issues = DigitalAssetPackPreflight.Run(entries);
        var issue = Assert.Single(issues);

        Assert.Equal("PG-REQUIRED-010", issue.RuleId);
        Assert.Equal(PreflightSeverity.ReviewRequired, issue.Severity);
        Assert.Equal("README.md", issue.RelativePath);
    }

    [Fact]
    public void Run_marks_case_insensitive_duplicate_paths_for_review()
    {
        var entries = new[]
        {
            new PackageInventoryEntry("README.md", PackageFileType.Text, 1, "AA", PackageInclusionState.Included, null),
            new PackageInventoryEntry(Path.Combine("assets", "logo.png"), PackageFileType.Binary, 1, "BB", PackageInclusionState.Included, null),
            new PackageInventoryEntry(Path.Combine("Assets", "LOGO.png"), PackageFileType.Binary, 1, "CC", PackageInclusionState.Included, null)
        };

        var issues = DigitalAssetPackPreflight.Run(entries);

        Assert.Contains(issues, issue =>
            issue.RuleId == "PG-PATH-021"
            && issue.Severity == PreflightSeverity.ReviewRequired
            && issue.RelativePath == Path.Combine("Assets", "LOGO.png"));
    }

    [Fact]
    public void Run_marks_absolute_local_path_metadata_for_review()
    {
        var entries = new[]
        {
            new PackageInventoryEntry("README.md", PackageFileType.Text, 1, "AA", PackageInclusionState.Included, null),
            new PackageInventoryEntry(
                RelativePath: "template.json",
                FileType: PackageFileType.Text,
                ByteLength: 1,
                Sha256: "BB",
                InclusionState: PackageInclusionState.Included,
                Reason: null,
                MetadataSignals: ["absolute-local-path"])
        };

        var issues = DigitalAssetPackPreflight.Run(entries);

        Assert.Contains(issues, issue =>
            issue.RuleId == "PG-METADATA-040"
            && issue.Severity == PreflightSeverity.ReviewRequired
            && issue.RelativePath == "template.json");
    }
}
