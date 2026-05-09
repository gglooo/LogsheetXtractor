using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LogsheetXtractor.Infrastructure.Services.Credentials;

public sealed class TemporaryCredentialFileCleanupHostedService(
    ITemporaryCredentialFileStore temporaryCredentialFileStore,
    ILogger<TemporaryCredentialFileCleanupHostedService> logger
) : IHostedService
{
    private static readonly TimeSpan StaleCredentialFileMinimumAge = TimeSpan.FromHours(24);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var staleCredentialFileCount = temporaryCredentialFileStore.CleanupStaleFiles(
            StaleCredentialFileMinimumAge
        );

        if (staleCredentialFileCount > 0)
        {
            logger.LogInformation(
                "Deleted {Count} stale temporary personal credential files.",
                staleCredentialFileCount
            );
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
