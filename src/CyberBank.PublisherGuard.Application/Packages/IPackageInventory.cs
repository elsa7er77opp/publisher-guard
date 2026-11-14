using CyberBank.PublisherGuard.Domain.Packages;

namespace CyberBank.PublisherGuard.Application.Packages;

public interface IPackageInventory
{
    IReadOnlyList<PackageInventoryEntry> Index(string sourceRoot);
}
