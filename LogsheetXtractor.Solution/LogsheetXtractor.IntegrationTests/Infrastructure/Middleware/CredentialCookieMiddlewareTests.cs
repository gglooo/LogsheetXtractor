using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Infrastructure.Middleware;
using Moq;
using Wolverine;

namespace LogsheetXtractor.IntegrationTests.Infrastructure.Middleware;

public class CredentialCookieMiddlewareTests
{
    [Fact]
    public async Task BeforeAsync_ShouldExposeBackgroundCredentials_WhenHandleIsValid()
    {
        var envelope = new Envelope(new object());
        envelope.Headers[CredentialsConstants.UserCredentialHandleHeaderName] = "credential-handle";
        var credentials = new Dictionary<ECredentialType, string>
        {
            [ECredentialType.Google] = "google-key",
        };
        var accessorMock = new Mock<ICredentialCookieAccessor>();
        var handleStoreMock = new Mock<IUserCredentialHandleStore>();
        var dbContextMock = new Mock<IAppDbContext>();
        handleStoreMock
            .Setup(s =>
                s.ResolveAsync(
                    "credential-handle",
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok<IReadOnlyDictionary<ECredentialType, string>>(credentials));

        await CredentialCookieMiddleware.BeforeAsync(
            envelope,
            accessorMock.Object,
            handleStoreMock.Object,
            dbContextMock.Object,
            CancellationToken.None
        );

        accessorMock.Verify(a => a.SetBackgroundCredentials(credentials), Times.Once);
        accessorMock.Verify(a => a.SetBackgroundCredentialError(It.IsAny<string>()), Times.Never);
        dbContextMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BeforeAsync_ShouldExposeExpectedError_WhenHandleIsInvalid()
    {
        var envelope = new Envelope(new object());
        envelope.Headers[CredentialsConstants.UserCredentialHandleHeaderName] = "credential-handle";
        var accessorMock = new Mock<ICredentialCookieAccessor>();
        var handleStoreMock = new Mock<IUserCredentialHandleStore>();
        var dbContextMock = new Mock<IAppDbContext>();
        handleStoreMock
            .Setup(s =>
                s.ResolveAsync(
                    "credential-handle",
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Result.Fail<IReadOnlyDictionary<ECredentialType, string>>(
                    new InvalidStateError(
                        CredentialsConstants.ExpiredBackgroundCredentialHandleMessage
                    )
                )
            );

        await CredentialCookieMiddleware.BeforeAsync(
            envelope,
            accessorMock.Object,
            handleStoreMock.Object,
            dbContextMock.Object,
            CancellationToken.None
        );

        accessorMock.Verify(
            a => a.SetBackgroundCredentialError(
                CredentialsConstants.ExpiredBackgroundCredentialHandleMessage
            ),
            Times.Once
        );
        accessorMock.Verify(
            a => a.SetBackgroundCredentials(It.IsAny<IReadOnlyDictionary<ECredentialType, string>>()),
            Times.Never
        );
        dbContextMock.Verify(d => d.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task BeforeAsync_ShouldNotResolveCredentials_WhenHandleIsMissing()
    {
        var envelope = new Envelope(new object());
        var accessorMock = new Mock<ICredentialCookieAccessor>();
        var handleStoreMock = new Mock<IUserCredentialHandleStore>();
        var dbContextMock = new Mock<IAppDbContext>();

        await CredentialCookieMiddleware.BeforeAsync(
            envelope,
            accessorMock.Object,
            handleStoreMock.Object,
            dbContextMock.Object,
            CancellationToken.None
        );

        handleStoreMock.Verify(
            s =>
                s.ResolveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        accessorMock.Verify(
            a => a.SetBackgroundCredentials(It.IsAny<IReadOnlyDictionary<ECredentialType, string>>()),
            Times.Never
        );
        dbContextMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
