using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WebFormHTR.Application.Features.Credentials;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Infrastructure.Services.Credentials;
using WebFormHTR.Infrastructure.Services.Storage;
using Xunit;

namespace WebFormHTR.Tests.Infrastructure.Services.Credentials;

public class CredentialContextProviderTests
{
    private readonly Mock<IOcrCredentialService> _ocrCredentialServiceMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<ICredentialCookieAccessor> _cookieAccessorMock;
    private readonly Mock<ILogger<UserCredentialContext>> _userContextLoggerMock;
    private readonly Mock<ILogger<CredentialContextProvider>> _loggerMock;
    private readonly CredentialContextProvider _provider;

    public CredentialContextProviderTests()
    {
        _ocrCredentialServiceMock = new Mock<IOcrCredentialService>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _cookieAccessorMock = new Mock<ICredentialCookieAccessor>();
        _userContextLoggerMock = new Mock<ILogger<UserCredentialContext>>();
        _loggerMock = new Mock<ILogger<CredentialContextProvider>>();

        _provider = new CredentialContextProvider(
            _ocrCredentialServiceMock.Object,
            _fileStorageServiceMock.Object,
            _cookieAccessorMock.Object,
            _userContextLoggerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetCredentialContextAsync_ShouldReturnUserCredentialContext_WhenValidCookieExists()
    {
        // Arrange
        var credentials = new Dictionary<ECredentialType, string>
        {
            { ECredentialType.Google, "some-google-key" },
            { ECredentialType.Azure, "some-azure-key" }
        };
        var cookieString = JsonSerializer.Serialize(credentials);
        _cookieAccessorMock.Setup(c => c.GetCookie()).Returns(cookieString);

        _fileStorageServiceMock.Setup(s => s.SaveTemporaryFileAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[] bytes, string name, CancellationToken ct) => $"temp/{name}");

        // Act
        var result = await _provider.GetCredentialContextAsync();

        // Assert
        result.Should().BeOfType<UserCredentialContext>();
        var userContext = (UserCredentialContext)result;
        
        userContext.CredentialPaths.Should().HaveCount(2);
        userContext.CredentialPaths.Should().Contain(p => p.Item1 == ECredentialType.Google && p.Item2.Contains("google"));
        userContext.CredentialPaths.Should().Contain(p => p.Item1 == ECredentialType.Azure && p.Item2.Contains("azure"));

        _fileStorageServiceMock.Verify(
            s => s.SaveTemporaryFileAsync(It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "some-google-key"), It.Is<string>(n => n.Contains("google")), It.IsAny<CancellationToken>()), 
            Times.Once);
        _fileStorageServiceMock.Verify(
            s => s.SaveTemporaryFileAsync(It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "some-azure-key"), It.Is<string>(n => n.Contains("azure")), It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task GetCredentialContextAsync_ShouldReturnSystemCredentialContext_WhenCookieIsNull()
    {
        // Arrange
        _cookieAccessorMock.Setup(c => c.GetCookie()).Returns((string?)null);

        var systemPaths = new List<(ECredentialType, string)>
        {
            (ECredentialType.Google, "system/google.json")
        };
        _ocrCredentialServiceMock.Setup(s => s.GetAvailableCredentialsPath()).Returns(systemPaths);

        // Act
        var result = await _provider.GetCredentialContextAsync();

        // Assert
        result.Should().BeOfType<SystemCredentialContext>();
        var systemContext = (SystemCredentialContext)result;
        systemContext.CredentialPaths.Should().BeEquivalentTo(systemPaths);
    }

    [Fact]
    public async Task GetCredentialContextAsync_ShouldReturnSystemCredentialContext_WhenCookieIsInvalid()
    {
        // Arrange
        _cookieAccessorMock.Setup(c => c.GetCookie()).Returns("invalid-json-string");

        var systemPaths = new List<(ECredentialType, string)>
        {
            (ECredentialType.Google, "system/google.json")
        };
        _ocrCredentialServiceMock.Setup(s => s.GetAvailableCredentialsPath()).Returns(systemPaths);

        // Act
        var result = await _provider.GetCredentialContextAsync();

        // Assert
        result.Should().BeOfType<SystemCredentialContext>();
        var systemContext = (SystemCredentialContext)result;
        systemContext.CredentialPaths.Should().BeEquivalentTo(systemPaths);
    }
}
