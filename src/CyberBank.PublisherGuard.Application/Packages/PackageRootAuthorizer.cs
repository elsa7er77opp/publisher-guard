namespace CyberBank.PublisherGuard.Application.Packages;

public static class PackageRootAuthorizer
{
    public static PackageRootAuthorizationResult Authorize(string sourceRoot, string outputParent)
    {
        var canonicalSourceRoot = Path.GetFullPath(sourceRoot);
        var canonicalOutputParent = Path.GetFullPath(outputParent);

        if (!Directory.Exists(canonicalSourceRoot))
        {
            return new PackageRootAuthorizationResult(
                IsAuthorized: false,
                SourceRoot: canonicalSourceRoot,
                OutputParent: canonicalOutputParent,
                Issues: ["PG-ROOT-001"]);
        }

        if (!Directory.Exists(canonicalOutputParent))
        {
            return new PackageRootAuthorizationResult(
                IsAuthorized: false,
                SourceRoot: canonicalSourceRoot,
                OutputParent: canonicalOutputParent,
                Issues: ["PG-ROOT-001"]);
        }

        if (IsSameOrChildPath(canonicalOutputParent, canonicalSourceRoot))
        {
            return new PackageRootAuthorizationResult(
                IsAuthorized: false,
                SourceRoot: canonicalSourceRoot,
                OutputParent: canonicalOutputParent,
                Issues: ["PG-ROOT-002"]);
        }

        return new PackageRootAuthorizationResult(
            IsAuthorized: true,
            SourceRoot: canonicalSourceRoot,
            OutputParent: canonicalOutputParent,
            Issues: Array.Empty<string>());
    }

    private static bool IsSameOrChildPath(string candidatePath, string rootPath)
    {
        var rootWithBoundary = rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        return candidatePath.Equals(rootPath, StringComparison.OrdinalIgnoreCase)
            || candidatePath.StartsWith(rootWithBoundary, StringComparison.OrdinalIgnoreCase);
    }
}

public sealed record PackageRootAuthorizationResult(
    bool IsAuthorized,
    string SourceRoot,
    string OutputParent,
    IReadOnlyList<string> Issues);
