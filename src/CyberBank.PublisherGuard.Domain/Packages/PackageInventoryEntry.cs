namespace CyberBank.PublisherGuard.Domain.Packages;

public sealed record PackageInventoryEntry(
    string RelativePath,
    PackageFileType FileType,
    long ByteLength,
    string Sha256,
    PackageInclusionState InclusionState,
    string? Reason,
    IReadOnlyList<string>? MetadataSignals = null);
