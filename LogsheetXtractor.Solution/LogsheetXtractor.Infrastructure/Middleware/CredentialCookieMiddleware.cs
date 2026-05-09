using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Interfaces;
using Wolverine;

namespace LogsheetXtractor.Infrastructure.Middleware;

public static class CredentialCookieMiddleware
{
    public static async Task BeforeAsync(
        Envelope envelope,
        ICredentialCookieAccessor cookieAccessor,
        IUserCredentialHandleStore credentialHandleStore,
        CancellationToken ct
    )
    {
        if (
            envelope.Headers.TryGetValue(
                CredentialsConstants.BackgroundHandleHeaderName,
                out var handle
            )
            && handle != null
        )
        {
            var result = await credentialHandleStore.ResolveAsync(handle, ct);
            if (result.IsSuccess)
            {
                cookieAccessor.SetBackgroundCredentials(result.Value);
                return;
            }

            cookieAccessor.SetBackgroundCredentialError(
                result.Errors.FirstOrDefault()?.Message
                    ?? CredentialsConstants.ExpiredBackgroundCredentialHandleMessage
            );
        }
    }

    public static async Task AfterAsync(
        Envelope envelope,
        IUserCredentialHandleStore credentialHandleStore,
        CancellationToken ct
    )
    {
        if (
            envelope.Headers.TryGetValue(
                CredentialsConstants.BackgroundHandleHeaderName,
                out var handle
            )
            && handle != null
        )
        {
            await credentialHandleStore.ReleaseAsync(handle, ct);
        }
    }
}
