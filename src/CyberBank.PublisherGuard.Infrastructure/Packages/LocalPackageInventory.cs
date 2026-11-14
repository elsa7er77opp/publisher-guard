using CyberBank.PublisherGuard.Application.Packages;
using CyberBank.PublisherGuard.Domain.Packages;

namespace CyberBank.PublisherGuard.Infrastructure.Packages;

public sealed class LocalPackageInventory : IPackageInventory
{
    public IReadOnlyList<PackageInventoryEntry> Index(string sourceRoot)
    {
        return StreamingPackageInventory.Index(sourceRoot);
    }
}
