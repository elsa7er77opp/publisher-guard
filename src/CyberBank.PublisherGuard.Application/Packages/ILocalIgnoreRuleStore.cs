using CyberBank.PublisherGuard.Domain.Packages;

namespace CyberBank.PublisherGuard.Application.Packages;

public interface ILocalIgnoreRuleStore
{
    IReadOnlyList<LocalIgnoreRule> Load();

    void Save(IReadOnlyList<LocalIgnoreRule> rules);
}
