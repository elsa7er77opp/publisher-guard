using System.Threading;
using System.Threading.Tasks;

namespace CyberBank.PublisherGuard.Application.Delivery;

public sealed class CleanPackageBuildQueue(ICleanPackageBuilder builder)
{
    private readonly SemaphoreSlim singleWorker = new(1, 1);

    public async Task<CleanPackageBuildOutcome> EnqueueAsync(CleanPackageBuildCommand command, CancellationToken cancellationToken)
    {
        await singleWorker.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await Task.Run(
                () => builder.Build(
                    command.SourceRoot,
                    command.OutputParent,
                    command.DeliveryName,
                    command.Manifest,
                    command.Decisions,
                    cancellationToken),
                cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            singleWorker.Release();
        }
    }
}
