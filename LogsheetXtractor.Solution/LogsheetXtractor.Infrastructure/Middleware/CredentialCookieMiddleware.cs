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
        IAppDbContext dbContext,
        CancellationToken ct
    )
    {
        if (
            envelope.Headers.TryGetValue(
                CredentialsConstants.UserCredentialHandleHeaderName,
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
            await dbContext.SaveChangesAsync(ct);
        }
    }
}
