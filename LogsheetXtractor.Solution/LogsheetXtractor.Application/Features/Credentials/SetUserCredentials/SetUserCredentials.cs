using FluentResults;
using LogsheetXtractor.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace LogsheetXtractor.Application.Features.Credentials.SetUserCredentials;

public sealed record SetUserCredentialsCommand(
    Dictionary<ECredentialType, string> Keys,
    string? PreviousHandle
);

public static class SetUserCredentialsHandler
{
    public static async Task<Result<string>> Handle(
        SetUserCredentialsCommand command,
        IUserCredentialHandleStore credentialHandleStore,
        IAppDbContext dbContext,
        IOptions<UserCredentialCookieOptions> cookieOptions,
        CancellationToken ct
    )
    {
        var ttl =
            cookieOptions.Value.Ttl > TimeSpan.Zero
                ? cookieOptions.Value.Ttl
                : TimeSpan.FromDays(365);

        var createResult = await credentialHandleStore.CreateAsync(
            command.Keys,
            ttl,
            ct
        );

        if (createResult.IsFailed)
        {
            return createResult;
        }

        await credentialHandleStore.ReleaseAsync(command.PreviousHandle, ct);

        await dbContext.SaveChangesAsync(ct);

        return createResult;
    }
}
