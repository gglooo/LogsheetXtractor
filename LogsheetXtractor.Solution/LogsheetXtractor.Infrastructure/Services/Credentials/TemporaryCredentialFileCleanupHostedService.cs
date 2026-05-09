using LogsheetXtractor.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LogsheetXtractor.Infrastructure.Services.Credentials;

public sealed class TemporaryCredentialFileCleanupHostedService(
    ITemporaryCredentialFileStore temporaryCredentialFileStore,
    IServiceScopeFactory scopeFactory,
    ILogger<TemporaryCredentialFileCleanupHostedService> logger
) : IHostedService
{
    private static readonly TimeSpan StaleCredentialFileMinimumAge = TimeSpan.FromHours(24);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var staleCredentialFileCount = temporaryCredentialFileStore.CleanupStaleFiles(
            StaleCredentialFileMinimumAge
        );

        await using var scope = scopeFactory.CreateAsyncScope();
        var credentialHandleStore =
            scope.ServiceProvider.GetRequiredService<IUserCredentialHandleStore>();
        var expiredCredentialHandleCount = await credentialHandleStore.CleanupExpiredAsync(
            cancellationToken
        );

        if (staleCredentialFileCount > 0)
        {
            logger.LogInformation(
                "Deleted {Count} stale temporary personal credential files.",
                staleCredentialFileCount
            );
        }

        if (expiredCredentialHandleCount > 0)
        {
            logger.LogInformation(
                "Deleted {Count} expired personal credential handles.",
                expiredCredentialHandleCount
            );
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
