using FluentResults;
using LogsheetXtractor.Application.Features.Credentials.DTOs;
using LogsheetXtractor.Application.Interfaces;

namespace LogsheetXtractor.Application.Features.Credentials;

public sealed record GetCredentialsStatusQuery(string? UserCredentialHandle);

public static class GetCredentialsStatusHandler
{
    public static async Task<Result<CredentialsStatusDto?>> Handle(
        GetCredentialsStatusQuery query,
        ICredentialService credentialsService,
        IUserCredentialHandleStore credentialHandleStore,
        IAppDbContext dbContext,
        CancellationToken ct)
    {
        var credentials = await credentialsService.GetAvailableCredentialTypesAsync(ct);
        var hasUserCredentials = false;

        if (query.UserCredentialHandle is not null)
        {
            var userCredentialsResult = await credentialHandleStore.ResolveAsync(
                query.UserCredentialHandle,
                ct
            );
            hasUserCredentials =
                userCredentialsResult.IsSuccess && userCredentialsResult.Value.Count > 0;

            if (userCredentialsResult.IsFailed)
            {
                await dbContext.SaveChangesAsync(ct);
            }
        }

        return new CredentialsStatusDto(credentials.Any() || hasUserCredentials, hasUserCredentials);
    }
}
