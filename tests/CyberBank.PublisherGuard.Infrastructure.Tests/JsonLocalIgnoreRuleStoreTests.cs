using CyberBank.PublisherGuard.Domain.Packages;
using CyberBank.PublisherGuard.Infrastructure.Packages;

namespace CyberBank.PublisherGuard.Infrastructure.Tests;

public sealed class JsonLocalIgnoreRuleStoreTests
{
    [Fact]
    public void Save_then_load_roundtrips_rules_in_the_configured_local_state_file()
    {
        var stateRoot = Path.Combine(Path.GetTempPath(), "CyberBank.PublisherGuard.Infrastructure.Tests", Guid.NewGuid().ToString("N"));
        var statePath = Path.Combine(stateRoot, "ignore-rules.json");
        var store = new JsonLocalIgnoreRuleStore(statePath);
        var expected = new LocalIgnoreRule(
            LocalIgnoreRuleKind.FileNamePattern,
            "*.bak",
            "Creator keeps backup copies out of delivery.",
            DateTimeOffset.UtcNow);

        try
        {
            store.Save([expected]);
            var loaded = Assert.Single(store.Load());

            Assert.Equal(expected, loaded);
            Assert.True(File.Exists(statePath));
        }
        finally
        {
            if (Directory.Exists(stateRoot))
            {
                Directory.Delete(stateRoot, recursive: true);
            }
        }
    }
}
