using CyberBank.PublisherGuard.Domain.Preflight;

namespace CyberBank.PublisherGuard.Domain.Tests;

public sealed class PreflightIssueTests
{
    [Fact]
    public void Issue_preserves_its_rule_severity_relative_path_and_explanation()
    {
        var issue = new PreflightIssue(
            RuleId: "PG-REQUIRED-010",
            Severity: PreflightSeverity.ReviewRequired,
            RelativePath: "README.md",
            Explanation: "A creator README is required for human review.");

        Assert.Equal("PG-REQUIRED-010", issue.RuleId);
        Assert.Equal(PreflightSeverity.ReviewRequired, issue.Severity);
        Assert.Equal("README.md", issue.RelativePath);
        Assert.Equal("A creator README is required for human review.", issue.Explanation);
    }
}
