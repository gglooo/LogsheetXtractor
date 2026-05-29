using System.Text;
using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Infrastructure.Services.Credentials;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LogsheetXtractor.IntegrationTests.Infrastructure.Services.Credentials;

public class CredentialContextProviderTests
{
    private readonly Mock<IOcrCredentialService> _ocrCredentialServiceMock;
    private readonly Mock<ITemporaryCredentialFileStore> _temporaryCredentialFileStoreMock;
    private readonly Mock<ICredentialCookieAccessor> _cookieAccessorMock;
    private readonly Mock<IUserCredentialHandleStore> _credentialHandleStoreMock;
    private readonly Mock<ILogger<UserCredentialContext>> _userContextLoggerMock;
    private readonly Mock<ILogger<CredentialContextProvider>> _loggerMock;
    private readonly CredentialContextProvider _provider;

    public CredentialContextProviderTests()
    {
        _ocrCredentialServiceMock = new Mock<IOcrCredentialService>();
        _temporaryCredentialFileStoreMock = new Mock<ITemporaryCredentialFileStore>();
        _cookieAccessorMock = new Mock<ICredentialCookieAccessor>();
        _credentialHandleStoreMock = new Mock<IUserCredentialHandleStore>();
        _userContextLoggerMock = new Mock<ILogger<UserCredentialContext>>();
        _loggerMock = new Mock<ILogger<CredentialContextProvider>>();

        _provider = new CredentialContextProvider(
            _ocrCredentialServiceMock.Object,
            _temporaryCredentialFileStoreMock.Object,
            _cookieAccessorMock.Object,
            _credentialHandleStoreMock.Object,
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
        const string handle = "0123456789abcdef0123456789abcdef";
        _cookieAccessorMock.Setup(c => c.GetCookie()).Returns(handle);
        _credentialHandleStoreMock
            .Setup(s =>
                s.ResolveAsync(
                    handle,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok<IReadOnlyDictionary<ECredentialType, string>>(credentials));

        var savedFileCounter = 0;
        _temporaryCredentialFileStoreMock
            .Setup(s => s.SaveAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => $"temp/{++savedFileCounter}.json");

        // Act
        var result = await _provider.GetCredentialContextAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<UserCredentialContext>();
        var userContext = (UserCredentialContext)result.Value;

        userContext.CredentialPaths.Should().HaveCount(2);
        userContext
            .CredentialPaths.Should()
            .Contain(p => p.Item1 == ECredentialType.Google && !p.Item2.Contains("google"));
        userContext
            .CredentialPaths.Should()
            .Contain(p => p.Item1 == ECredentialType.Azure && !p.Item2.Contains("azure"));

        _temporaryCredentialFileStoreMock.Verify(
            s => s.SaveAsync(
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "some-google-key"),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        _temporaryCredentialFileStoreMock.Verify(
            s => s.SaveAsync(
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "some-azure-key"),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetCredentialContextAsync_ShouldPreferActiveHttpCookie_WhenBackgroundCredentialsExist()
    {
        var httpCredentials = new Dictionary<ECredentialType, string>
        {
            { ECredentialType.Google, "http-google-key" },
        };
        var backgroundCredentials = new Dictionary<ECredentialType, string>
        {
            { ECredentialType.Google, "background-google-key" },
        };
        const string handle = "0123456789abcdef0123456789abcdef";
        _cookieAccessorMock.Setup(c => c.GetCookie()).Returns(handle);
        _cookieAccessorMock
            .Setup(c => c.GetBackgroundCredentials())
            .Returns(backgroundCredentials);
        _credentialHandleStoreMock
            .Setup(s => s.ResolveAsync(handle, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyDictionary<ECredentialType, string>>(httpCredentials));
        _temporaryCredentialFileStoreMock
            .Setup(s => s.SaveAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("temp/http.json");

        var result = await _provider.GetCredentialContextAsync();

        result.IsSuccess.Should().BeTrue();
        _temporaryCredentialFileStoreMock.Verify(
            s => s.SaveAsync(
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "http-google-key"),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        _temporaryCredentialFileStoreMock.Verify(
            s => s.SaveAsync(
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "background-google-key"),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
    }

    [Fact]
    public async Task GetCredentialContextAsync_ShouldUseBackgroundCredentials_WhenHttpCookieIsMissing()
    {
        var backgroundCredentials = new Dictionary<ECredentialType, string>
        {
            { ECredentialType.Google, "background-google-key" },
        };
        _cookieAccessorMock.Setup(c => c.GetCookie()).Returns((string?)null);
        _cookieAccessorMock
            .Setup(c => c.GetBackgroundCredentials())
            .Returns(backgroundCredentials);
        _temporaryCredentialFileStoreMock
            .Setup(s => s.SaveAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("temp/background.json");

        var result = await _provider.GetCredentialContextAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<UserCredentialContext>();
        _temporaryCredentialFileStoreMock.Verify(
            s => s.SaveAsync(
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "background-google-key"),
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
    public async Task GetCredentialContextAsync_ShouldFail_WhenBackgroundCredentialsAreInvalid()
    {
        // Arrange
        _cookieAccessorMock
            .Setup(c => c.GetBackgroundCredentialError())
            .Returns(CredentialsConstants.ExpiredBackgroundCredentialHandleMessage);

        // Act
        var result = await _provider.GetCredentialContextAsync();

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e is InvalidStateError
            && e.Message == CredentialsConstants.ExpiredBackgroundCredentialHandleMessage
        );
        _ocrCredentialServiceMock.Verify(s => s.GetAvailableCredentialsPath(), Times.Never);
    }
}
