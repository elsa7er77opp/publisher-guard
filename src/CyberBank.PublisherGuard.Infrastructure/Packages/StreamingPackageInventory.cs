using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using CyberBank.PublisherGuard.Domain.Packages;

namespace CyberBank.PublisherGuard.Infrastructure.Packages;

public static class StreamingPackageInventory
{
    private static readonly Regex AbsoluteWindowsPathPattern = new(@"(?:[A-Za-z]:[\\/]|\\\\)", RegexOptions.CultureInvariant);

    public static IReadOnlyList<PackageInventoryEntry> Index(string sourceRoot)
    {
        return Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories)
            .Select(filePath => CreateEntry(sourceRoot, filePath))
            .ToArray();
    }

    private static PackageInventoryEntry CreateEntry(string sourceRoot, string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var hash = SHA256.HashData(stream);
        var fileInfo = new FileInfo(filePath);
        var isWindowsThumbnailArtifact = Path.GetFileName(filePath).Equals("Thumbs.db", StringComparison.OrdinalIgnoreCase);
        var fileType = Classify(filePath);
        var isExecutablePayload = fileType == PackageFileType.Executable;
        var metadataSignals = DetectMetadataSignals(stream, fileType);

        return new PackageInventoryEntry(
            RelativePath: Path.GetRelativePath(sourceRoot, filePath),
            FileType: fileType,
            ByteLength: fileInfo.Length,
            Sha256: Convert.ToHexString(hash),
            InclusionState: isWindowsThumbnailArtifact || isExecutablePayload ? PackageInclusionState.Excluded : PackageInclusionState.Included,
            Reason: isWindowsThumbnailArtifact ? "PG-NAME-020" : isExecutablePayload ? "PG-CONTENT-050" : null,
            MetadataSignals: metadataSignals);
    }

    private static PackageFileType Classify(string filePath)
    {
        if (new[] { ".exe", ".bat", ".cmd", ".msi", ".ps1" }
            .Contains(Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase))
        {
            return PackageFileType.Executable;
        }

        return new[] { ".txt", ".md", ".json", ".yaml", ".yml", ".xml", ".csv" }
            .Contains(Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase)
            ? PackageFileType.Text
            : PackageFileType.Binary;
    }

    private static IReadOnlyList<string> DetectMetadataSignals(Stream stream, PackageFileType fileType)
    {
        if (fileType != PackageFileType.Text)
        {
            return Array.Empty<string>();
        }

        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: true);
        var buffer = new char[4096];

        while (reader.Read(buffer, 0, buffer.Length) is var readCount && readCount > 0)
        {
            if (AbsoluteWindowsPathPattern.IsMatch(buffer.AsSpan(0, readCount)))
            {
                return ["absolute-local-path"];
            }
        }

        return Array.Empty<string>();
    }
}
