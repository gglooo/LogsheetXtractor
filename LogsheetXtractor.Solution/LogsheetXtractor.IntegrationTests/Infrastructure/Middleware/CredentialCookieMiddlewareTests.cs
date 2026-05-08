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
    public void Before_ShouldExposeUnprotectedBackgroundCredentials_WhenSnapshotIsValid()
    {
        var envelope = new Envelope(new object());
        envelope.Headers[CredentialsConstants.BackgroundSnapshotHeaderName] = "v1:snapshot";
        var credentials = new Dictionary<ECredentialType, string>
        {
            [ECredentialType.Google] = "google-key",
        };
        var accessorMock = new Mock<ICredentialCookieAccessor>();
        var snapshotProtectorMock = new Mock<IUserCredentialSnapshotProtector>();
        snapshotProtectorMock
            .Setup(p => p.Unprotect("v1:snapshot"))
            .Returns(Result.Ok(credentials));

        CredentialCookieMiddleware.Before(
            envelope,
            accessorMock.Object,
            snapshotProtectorMock.Object
        );

        accessorMock.Verify(a => a.SetBackgroundCredentials(credentials), Times.Once);
        accessorMock.Verify(a => a.SetBackgroundCredentialError(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Before_ShouldExposeExpectedError_WhenSnapshotIsInvalid()
    {
        var envelope = new Envelope(new object());
        envelope.Headers[CredentialsConstants.BackgroundSnapshotHeaderName] = "v1:snapshot";
        var accessorMock = new Mock<ICredentialCookieAccessor>();
        var snapshotProtectorMock = new Mock<IUserCredentialSnapshotProtector>();
        snapshotProtectorMock
            .Setup(p => p.Unprotect("v1:snapshot"))
            .Returns(
                Result.Fail<Dictionary<ECredentialType, string>>(
                    new InvalidStateError(CredentialsConstants.ExpiredBackgroundSnapshotMessage)
                )
            );

        CredentialCookieMiddleware.Before(
            envelope,
            accessorMock.Object,
            snapshotProtectorMock.Object
        );

        accessorMock.Verify(
            a => a.SetBackgroundCredentialError(CredentialsConstants.ExpiredBackgroundSnapshotMessage),
            Times.Once
        );
        accessorMock.Verify(
            a => a.SetBackgroundCredentials(It.IsAny<IReadOnlyDictionary<ECredentialType, string>>()),
            Times.Never
        );
    }
}
