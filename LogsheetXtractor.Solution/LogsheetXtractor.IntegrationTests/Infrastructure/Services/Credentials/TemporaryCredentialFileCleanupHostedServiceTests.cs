using FluentAssertions;
using LogsheetXtractor.Infrastructure.Services.Credentials;
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
        var logger = new Mock<ILogger<TemporaryCredentialFileCleanupHostedService>>();
        var service = new TemporaryCredentialFileCleanupHostedService(
            fileStore.Object,
            logger.Object
        );

        await service.StartAsync(CancellationToken.None);

        fileStore.Verify(s => s.CleanupStaleFiles(TimeSpan.FromHours(24)), Times.Once);
    }

    [Fact]
    public async Task StopAsync_ShouldComplete()
    {
        var fileStore = new Mock<ITemporaryCredentialFileStore>();
        var logger = new Mock<ILogger<TemporaryCredentialFileCleanupHostedService>>();
        var service = new TemporaryCredentialFileCleanupHostedService(
            fileStore.Object,
            logger.Object
        );

        var act = async () => await service.StopAsync(CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
