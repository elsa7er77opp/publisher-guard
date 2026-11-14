using CyberBank.PublisherGuard.Domain.Packages;

namespace CyberBank.PublisherGuard.Domain.Evidence;

public sealed record CleanPackageManifest(
    PackageProfile Profile,
    IReadOnlyList<PackageInventoryEntry> Entries)
{
    public static CleanPackageManifest Create(PackageProfile profile, IEnumerable<PackageInventoryEntry> finalEntries)
    {
        return new CleanPackageManifest(
            profile,
            finalEntries
                .Where(entry => entry.InclusionState == PackageInclusionState.Included)
                .ToArray());
    }
}
