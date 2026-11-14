namespace CyberBank.PublisherGuard.Domain.Packages;

public sealed record LocalIgnoreRule(
    LocalIgnoreRuleKind Kind,
    string Pattern,
    string Reason,
    DateTimeOffset CreatedAtUtc);
