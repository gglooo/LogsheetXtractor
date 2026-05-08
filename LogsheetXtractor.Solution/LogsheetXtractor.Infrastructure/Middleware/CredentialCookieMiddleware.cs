using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Interfaces;
using Wolverine;

namespace LogsheetXtractor.Infrastructure.Middleware;

public static class CredentialCookieMiddleware
{
    public static void Before(
        Envelope envelope,
        ICredentialCookieAccessor cookieAccessor,
        IUserCredentialSnapshotProtector snapshotProtector
    )
    {
        if (
            envelope.Headers.TryGetValue(
                CredentialsConstants.BackgroundSnapshotHeaderName,
                out var snapshot
            )
            && snapshot != null
        )
        {
            var result = snapshotProtector.Unprotect(snapshot);
            if (result.IsSuccess)
            {
                cookieAccessor.SetBackgroundCredentials(result.Value);
                return;
            }

            cookieAccessor.SetBackgroundCredentialError(
                result.Errors.FirstOrDefault()?.Message
                    ?? CredentialsConstants.ExpiredBackgroundSnapshotMessage
            );
        }
    }
}
