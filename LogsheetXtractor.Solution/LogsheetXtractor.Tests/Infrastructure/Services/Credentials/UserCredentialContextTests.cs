using FluentAssertions;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Infrastructure.Services.Credentials;
using LogsheetXtractor.Infrastructure.Services.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LogsheetXtractor.Tests.Infrastructure.Services.Credentials;

public class UserCredentialContextTests
{
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<ILogger<UserCredentialContext>> _loggerMock;

    public UserCredentialContextTests()
    {
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _loggerMock = new Mock<ILogger<UserCredentialContext>>();
    }

    [Fact]
    public void Constructor_ShouldSetCredentialPaths()
    {
        // Arrange
        var paths = new List<(ECredentialType, string)>
        {
            (ECredentialType.Google, "temp/google.json"),
            (ECredentialType.Azure, "temp/azure.json"),
        };

        // Act
        var context = new UserCredentialContext(
            paths,
            _fileStorageServiceMock.Object,
            _loggerMock.Object
        );

        // Assert
        context.CredentialPaths.Should().BeEquivalentTo(paths);
    }

    [Fact]
    public async Task DisposeAsync_ShouldDeleteAllTemporaryFiles()
    {
        // Arrange
        var paths = new List<(ECredentialType, string)>
        {
            (ECredentialType.Google, "temp/google.json"),
            (ECredentialType.Azure, "temp/azure.json"),
        };
        var context = new UserCredentialContext(
            paths,
            _fileStorageServiceMock.Object,
            _loggerMock.Object
        );

        _fileStorageServiceMock.Setup(s => s.DeleteFile(It.IsAny<string>())).Returns(true);

        // Act
        await context.DisposeAsync();

        // Assert
        _fileStorageServiceMock.Verify(s => s.DeleteFile("temp/google.json"), Times.Once);
        _fileStorageServiceMock.Verify(s => s.DeleteFile("temp/azure.json"), Times.Once);
    }

    [Fact]
    public async Task DisposeAsync_ShouldHandleExceptionsDuringDeletion()
    {
        // Arrange
        var paths = new List<(ECredentialType, string)>
        {
            (ECredentialType.Google, "temp/google.json"),
            (ECredentialType.Azure, "temp/azure.json"),
        };
        var context = new UserCredentialContext(
            paths,
            _fileStorageServiceMock.Object,
            _loggerMock.Object
        );

        _fileStorageServiceMock
            .Setup(s => s.DeleteFile("temp/google.json"))
            .Throws(new Exception("Mock exception"));
        _fileStorageServiceMock.Setup(s => s.DeleteFile("temp/azure.json")).Returns(true);

        // Act
        var act = async () => await context.DisposeAsync();

        // Assert
        await act.Should().NotThrowAsync();
        _fileStorageServiceMock.Verify(s => s.DeleteFile("temp/google.json"), Times.Once);
        _fileStorageServiceMock.Verify(s => s.DeleteFile("temp/azure.json"), Times.Once);
    }
}
