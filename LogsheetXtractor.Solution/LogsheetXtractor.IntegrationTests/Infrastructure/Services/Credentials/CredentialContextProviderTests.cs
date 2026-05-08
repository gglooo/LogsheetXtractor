using System.Text;
using FluentAssertions;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Infrastructure.Services.Credentials;
using LogsheetXtractor.Infrastructure.Services.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LogsheetXtractor.IntegrationTests.Infrastructure.Services.Credentials;

public class CredentialContextProviderTests
{
    private readonly Mock<IOcrCredentialService> _ocrCredentialServiceMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<ICredentialCookieAccessor> _cookieAccessorMock;
    private readonly Mock<IUserCredentialCookieProtector> _credentialCookieProtectorMock;
    private readonly Mock<ILogger<UserCredentialContext>> _userContextLoggerMock;
    private readonly Mock<ILogger<CredentialContextProvider>> _loggerMock;
    private readonly CredentialContextProvider _provider;

    public CredentialContextProviderTests()
    {
        _ocrCredentialServiceMock = new Mock<IOcrCredentialService>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _cookieAccessorMock = new Mock<ICredentialCookieAccessor>();
        _credentialCookieProtectorMock = new Mock<IUserCredentialCookieProtector>();
        _userContextLoggerMock = new Mock<ILogger<UserCredentialContext>>();
        _loggerMock = new Mock<ILogger<CredentialContextProvider>>();

        _provider = new CredentialContextProvider(
            _ocrCredentialServiceMock.Object,
            _fileStorageServiceMock.Object,
            _cookieAccessorMock.Object,
            _credentialCookieProtectorMock.Object,
            _userContextLoggerMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GetCredentialContextAsync_ShouldReturnUserCredentialContext_WhenValidCookieExists()
    {
        // Arrange
        var credentials = new Dictionary<ECredentialType, string>
        {
            { ECredentialType.Google, "some-google-key" },
            { ECredentialType.Azure, "some-azure-key" },
        };
        const string cookieString = "v1:protected-cookie";
        _cookieAccessorMock.Setup(c => c.GetCookie()).Returns(cookieString);
        _credentialCookieProtectorMock
            .Setup(p => p.Unprotect(cookieString))
            .Returns(credentials);

        _fileStorageServiceMock
            .Setup(s =>
                s.SaveTemporaryFileAsync(
                    It.IsAny<byte[]>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((byte[] bytes, string name, CancellationToken ct) => $"temp/{name}");

        // Act
        var result = await _provider.GetCredentialContextAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<UserCredentialContext>();
        var userContext = (UserCredentialContext)result.Value;

        userContext.CredentialPaths.Should().HaveCount(2);
        userContext
            .CredentialPaths.Should()
            .Contain(p => p.Item1 == ECredentialType.Google && p.Item2.Contains("google"));
        userContext
            .CredentialPaths.Should()
            .Contain(p => p.Item1 == ECredentialType.Azure && p.Item2.Contains("azure"));

        _fileStorageServiceMock.Verify(
            s =>
                s.SaveTemporaryFileAsync(
                    It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "some-google-key"),
                    It.Is<string>(n => n.Contains("google")),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _fileStorageServiceMock.Verify(
            s =>
                s.SaveTemporaryFileAsync(
                    It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "some-azure-key"),
                    It.Is<string>(n => n.Contains("azure")),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetCredentialContextAsync_ShouldReturnSystemCredentialContext_WhenCookieIsNull()
    {
        // Arrange
        _cookieAccessorMock.Setup(c => c.GetCookie()).Returns((string?)null);

        var systemPaths = new List<(ECredentialType, string)>
        {
            (ECredentialType.Google, "system/google.json"),
        };
        _ocrCredentialServiceMock.Setup(s => s.GetAvailableCredentialsPath()).Returns(systemPaths);

        // Act
        var result = await _provider.GetCredentialContextAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<SystemCredentialContext>();
        var systemContext = (SystemCredentialContext)result.Value;
        systemContext.CredentialPaths.Should().BeEquivalentTo(systemPaths);
    }

    [Fact]
    public async Task GetCredentialContextAsync_ShouldReturnSystemCredentialContext_WhenCookieIsInvalid()
    {
        // Arrange
        _cookieAccessorMock.Setup(c => c.GetCookie()).Returns("invalid-json-string");

        var systemPaths = new List<(ECredentialType, string)>
        {
            (ECredentialType.Google, "system/google.json"),
        };
        _ocrCredentialServiceMock.Setup(s => s.GetAvailableCredentialsPath()).Returns(systemPaths);

        // Act
        var result = await _provider.GetCredentialContextAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<SystemCredentialContext>();
        var systemContext = (SystemCredentialContext)result.Value;
        systemContext.CredentialPaths.Should().BeEquivalentTo(systemPaths);
    }

    [Fact]
    public async Task GetCredentialContextAsync_ShouldFail_WhenBackgroundSnapshotIsInvalid()
    {
        // Arrange
        _cookieAccessorMock
            .Setup(c => c.GetBackgroundCredentialError())
            .Returns(CredentialsConstants.ExpiredBackgroundSnapshotMessage);

        // Act
        var result = await _provider.GetCredentialContextAsync();

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e is InvalidStateError
            && e.Message == CredentialsConstants.ExpiredBackgroundSnapshotMessage
        );
        _ocrCredentialServiceMock.Verify(s => s.GetAvailableCredentialsPath(), Times.Never);
    }
}
