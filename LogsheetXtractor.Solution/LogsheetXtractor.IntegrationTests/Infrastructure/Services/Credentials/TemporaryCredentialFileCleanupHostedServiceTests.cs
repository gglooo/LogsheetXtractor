using FluentAssertions;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Infrastructure.Services.Credentials;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LogsheetXtractor.IntegrationTests.Infrastructure.Services.Credentials;

public sealed class TemporaryCredentialFileCleanupHostedServiceTests
{
    [Fact]
    public async Task StartAsync_ShouldCleanStaleCredentialFiles()
    {
        var fileStore = new Mock<ITemporaryCredentialFileStore>();
        var handleStore = new Mock<IUserCredentialHandleStore>();
        handleStore
            .Setup(s => s.CleanupExpiredAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        await using var serviceProvider = new ServiceCollection()
            .AddScoped(_ => handleStore.Object)
            .BuildServiceProvider();
        var logger = new Mock<ILogger<TemporaryCredentialFileCleanupHostedService>>();
        var service = new TemporaryCredentialFileCleanupHostedService(
            fileStore.Object,
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            logger.Object
        );

        await service.StartAsync(CancellationToken.None);

        fileStore.Verify(s => s.CleanupStaleFiles(TimeSpan.FromHours(24)), Times.Once);
        handleStore.Verify(s => s.CleanupExpiredAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task StopAsync_ShouldComplete()
    {
        var fileStore = new Mock<ITemporaryCredentialFileStore>();
        var handleStore = new Mock<IUserCredentialHandleStore>();
        handleStore
            .Setup(s => s.CleanupExpiredAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        await using var serviceProvider = new ServiceCollection()
            .AddScoped(_ => handleStore.Object)
            .BuildServiceProvider();
        var logger = new Mock<ILogger<TemporaryCredentialFileCleanupHostedService>>();
        var service = new TemporaryCredentialFileCleanupHostedService(
            fileStore.Object,
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            logger.Object
        );

        var act = async () => await service.StopAsync(CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
