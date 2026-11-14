using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using CyberBank.PublisherGuard.Domain.Packages;

namespace CyberBank.PublisherGuard.Infrastructure.Delivery;

public static class AtomicCleanPackageBuilder
{
    public static CleanPackageBuildResult Build(CleanPackageBuildRequest request, CancellationToken cancellationToken = default)
    {
        var sourceRoot = Path.GetFullPath(request.SourceRoot);
        var outputParent = Path.GetFullPath(request.OutputParent);
        ValidateRequest(sourceRoot, outputParent, request.DeliveryName);

        var outputRoot = Path.Combine(outputParent, request.DeliveryName);
        if (Directory.Exists(outputRoot) || File.Exists(outputRoot))
        {
            throw new IOException("The requested clean delivery output already exists.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        var stagingRoot = Path.Combine(outputParent, $".publisher-guard-staging-{Guid.NewGuid():N}");
        Directory.CreateDirectory(stagingRoot);

        try
        {
            foreach (var entry in request.Manifest.Entries.Where(entry => entry.InclusionState == PackageInclusionState.Included))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var sourcePath = ResolveContainedPath(sourceRoot, entry.RelativePath);
                var destinationPath = ResolveContainedPath(stagingRoot, entry.RelativePath);
                var destinationDirectory = Path.GetDirectoryName(destinationPath);

                if (destinationDirectory is not null)
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                File.Copy(sourcePath, destinationPath, overwrite: false);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var evidence = new
            {
                schema = "cyberbank.publisher-guard.clean-package/v1",
                profile = request.Manifest.Profile.Id,
                entries = request.Manifest.Entries
                    .Where(entry => entry.InclusionState == PackageInclusionState.Included)
                    .Select(entry => new
                    {
                        relativePath = entry.RelativePath,
                        type = entry.FileType.ToString(),
                        bytes = entry.ByteLength,
                        sha256 = entry.Sha256
                    })
            };
            File.WriteAllText(
                Path.Combine(stagingRoot, "publisher-guard.manifest.json"),
                JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true }));

            cancellationToken.ThrowIfCancellationRequested();
            var audit = new
            {
                eventType = "clean-delivery-complete",
                completedAtUtc = DateTimeOffset.UtcNow,
                profile = request.Manifest.Profile.Id,
                includedEntryCount = request.Manifest.Entries.Count(entry => entry.InclusionState == PackageInclusionState.Included),
                decisions = (request.Decisions ?? Array.Empty<Domain.Evidence.DeliveryDecision>())
                    .Select(decision => new
                    {
                        relativePath = decision.RelativePath,
                        inclusionState = decision.InclusionState.ToString(),
                        reason = decision.Reason,
                        decidedAtUtc = decision.DecidedAtUtc
                    })
            };
            File.WriteAllText(
                Path.Combine(stagingRoot, "publisher-guard.audit.json"),
                JsonSerializer.Serialize(audit, new JsonSerializerOptions { WriteIndented = true }));

            cancellationToken.ThrowIfCancellationRequested();
            File.WriteAllText(
                Path.Combine(stagingRoot, "delivery-report.md"),
                BuildDeliveryReport(request));

            cancellationToken.ThrowIfCancellationRequested();
            Directory.Move(stagingRoot, outputRoot);
            return new CleanPackageBuildResult(IsComplete: true, OutputRoot: outputRoot);
        }
        catch
        {
            if (Directory.Exists(stagingRoot))
            {
                Directory.Delete(stagingRoot, recursive: true);
            }

            throw;
        }
    }

    private static string BuildDeliveryReport(CleanPackageBuildRequest request)
    {
        var includedEntries = request.Manifest.Entries
            .Where(entry => entry.InclusionState == PackageInclusionState.Included)
            .ToArray();
        var keptOutDecisions = (request.Decisions ?? Array.Empty<Domain.Evidence.DeliveryDecision>())
            .Where(decision => decision.InclusionState == PackageInclusionState.Excluded)
            .GroupBy(decision => decision.RelativePath, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.Last())
            .ToArray();

        var lines = new List<string>
        {
            "# Delivery report",
            string.Empty,
            "This report describes the local clean delivery created by Publisher Guard.",
            string.Empty,
            "## Summary",
            $"- {includedEntries.Length} included file{(includedEntries.Length == 1 ? string.Empty : "s")}",
            $"- {keptOutDecisions.Length} kept-out file{(keptOutDecisions.Length == 1 ? string.Empty : "s")}",
            "- Evidence: publisher-guard.manifest.json and publisher-guard.audit.json",
            string.Empty,
            "## Included files"
        };

        lines.AddRange(includedEntries.Select(entry => $"- {entry.RelativePath}"));

        lines.Add(string.Empty);
        lines.Add("## Kept out");
        lines.AddRange(keptOutDecisions.Select(decision => $"- {decision.RelativePath}{(string.IsNullOrWhiteSpace(decision.Reason) ? string.Empty : $" — {decision.Reason}")}"));

        lines.Add(string.Empty);
        lines.Add("## Verify this delivery");
        lines.Add("Compare each delivered file with the SHA-256 values in publisher-guard.manifest.json.");
        lines.Add("This report records a local package review. It does not represent legal approval or marketplace acceptance.");

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static void ValidateRequest(string sourceRoot, string outputParent, string deliveryName)
    {
        if (!Directory.Exists(sourceRoot) || !Directory.Exists(outputParent))
        {
            throw new DirectoryNotFoundException("Source root and output parent must both exist locally.");
        }

        if (string.IsNullOrWhiteSpace(deliveryName) || !deliveryName.Equals(Path.GetFileName(deliveryName), StringComparison.Ordinal))
        {
            throw new ArgumentException("Delivery name must be a single directory name.", nameof(deliveryName));
        }

        var outputRoot = Path.Combine(outputParent, deliveryName);
        if (IsSameOrNested(sourceRoot, outputRoot))
        {
            throw new InvalidOperationException("Clean delivery output cannot be created inside the source package root.");
        }
    }

    private static string ResolveContainedPath(string root, string relativePath)
    {
        var candidate = Path.GetFullPath(Path.Combine(root, relativePath));
        if (!IsSameOrNested(root, candidate))
        {
            throw new InvalidOperationException("Manifest entry escapes the authorized package root.");
        }

        return candidate;
    }

    private static bool IsSameOrNested(string root, string candidate)
    {
        var normalizedRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        var normalizedCandidate = Path.TrimEndingDirectorySeparator(Path.GetFullPath(candidate));
        return normalizedCandidate.Equals(normalizedRoot, StringComparison.OrdinalIgnoreCase)
            || normalizedCandidate.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || normalizedCandidate.StartsWith(normalizedRoot + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }
}
