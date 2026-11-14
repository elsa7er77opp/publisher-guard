using CyberBank.PublisherGuard.Domain.Packages;

namespace CyberBank.PublisherGuard.Application.Packages;

public static class PackageInventoryComparer
{
    public static PackageInventoryComparison Compare(
        IReadOnlyList<PackageInventoryEntry> before,
        IReadOnlyList<PackageInventoryEntry> after)
    {
        var beforeByPath = before.ToDictionary(entry => entry.RelativePath, StringComparer.OrdinalIgnoreCase);
        var afterByPath = after.ToDictionary(entry => entry.RelativePath, StringComparer.OrdinalIgnoreCase);

        var added = afterByPath.Keys
            .Where(path => !beforeByPath.ContainsKey(path))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var removed = beforeByPath.Keys
            .Where(path => !afterByPath.ContainsKey(path))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var changed = afterByPath
            .Where(pair => beforeByPath.TryGetValue(pair.Key, out var previous)
                           && !previous.Sha256.Equals(pair.Value.Sha256, StringComparison.OrdinalIgnoreCase))
            .Select(pair => pair.Value.RelativePath)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new PackageInventoryComparison(added, removed, changed);
    }
}
