namespace CyberBank.PublisherGuard.Domain.Preflight;

public sealed record PreflightIssue(
    string RuleId,
    PreflightSeverity Severity,
    string RelativePath,
    string Explanation);
