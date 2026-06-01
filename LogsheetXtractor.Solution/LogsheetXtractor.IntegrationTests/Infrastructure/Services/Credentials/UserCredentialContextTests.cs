using FluentAssertions;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Infrastructure.Services.Credentials;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LogsheetXtractor.IntegrationTests.Infrastructure.Services.Credentials;

public class UserCredentialContextTests
{
    private readonly Mock<ITemporaryCredentialFileStore> _temporaryCredentialFileStoreMock;
    private readonly Mock<ILogger<UserCredentialContext>> _loggerMock;

    public UserCredentialContextTests()
    {
        _temporaryCredentialFileStoreMock = new Mock<ITemporaryCredentialFileStore>();
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
            _temporaryCredentialFileStoreMock.Object,
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
            _temporaryCredentialFileStoreMock.Object,
            _loggerMock.Object
        );

        _temporaryCredentialFileStoreMock.Setup(s => s.Delete(It.IsAny<string>())).Returns(true);

        // Act
        await context.DisposeAsync();

        // Assert
        _temporaryCredentialFileStoreMock.Verify(s => s.Delete("temp/google.json"), Times.Once);
        _temporaryCredentialFileStoreMock.Verify(s => s.Delete("temp/azure.json"), Times.Once);
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
            _temporaryCredentialFileStoreMock.Object,
            _loggerMock.Object
        );

        _temporaryCredentialFileStoreMock
            .Setup(s => s.Delete("temp/google.json"))
            .Throws(new Exception("Mock exception"));
        _temporaryCredentialFileStoreMock.Setup(s => s.Delete("temp/azure.json")).Returns(true);

        // Act
        var act = async () => await context.DisposeAsync();

        // Assert
        await act.Should().NotThrowAsync();
        _temporaryCredentialFileStoreMock.Verify(s => s.Delete("temp/google.json"), Times.Once);
        _temporaryCredentialFileStoreMock.Verify(s => s.Delete("temp/azure.json"), Times.Once);
    }
}
