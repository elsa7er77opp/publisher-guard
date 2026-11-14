using System.Text.Json;
using CyberBank.PublisherGuard.Application.Packages;
using CyberBank.PublisherGuard.Domain.Packages;

namespace CyberBank.PublisherGuard.Infrastructure.Packages;

public sealed class JsonLocalIgnoreRuleStore(string statePath) : ILocalIgnoreRuleStore
{
    public IReadOnlyList<LocalIgnoreRule> Load()
    {
        if (!File.Exists(statePath))
        {
            return Array.Empty<LocalIgnoreRule>();
        }

        return JsonSerializer.Deserialize<List<LocalIgnoreRule>>(File.ReadAllText(statePath))
            ?? new List<LocalIgnoreRule>();
    }

    public void Save(IReadOnlyList<LocalIgnoreRule> rules)
    {
        var directory = Path.GetDirectoryName(statePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var temporaryPath = statePath + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(rules, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temporaryPath, statePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}
