using System.Text;
using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace LogsheetXtractor.Infrastructure.Services.Credentials;

public class CredentialContextProvider(
    IOcrCredentialService ocrCredentialService,
    ITemporaryCredentialFileStore temporaryCredentialFileStore,
    ICredentialCookieAccessor cookieAccessor,
    IUserCredentialHandleStore credentialHandleStore,
    ILogger<UserCredentialContext> userContextLogger,
    ILogger<CredentialContextProvider> logger
) : ICredentialContextProvider
{
    public async Task<Result<ICredentialContext>> GetCredentialContextAsync(
        CancellationToken ct = default
    )
    {
        var keys = await ResolveActiveHttpCredentialsAsync(ct);
        if (keys is null)
        {
            var backgroundError = cookieAccessor.GetBackgroundCredentialError();
            if (!string.IsNullOrWhiteSpace(backgroundError))
            {
                logger.LogWarning(
                    "Background user credential handle could not be used: {Error}",
                    backgroundError
                );
                return Result.Fail<ICredentialContext>(new InvalidStateError(backgroundError));
            }

            keys = cookieAccessor.GetBackgroundCredentials();
        }

        if (keys is null)
        {
            logger.LogInformation(
                "No valid user credentials found. Falling back to system credentials."
            );
            return Result.Ok<ICredentialContext>(
                new SystemCredentialContext(ocrCredentialService.GetAvailableCredentialsPath())
            );
        }

        logger.LogInformation(
            "Found active user credentials. Creating temporary credential files."
        );
        var tempPaths = new List<(ECredentialType, string)>();

        foreach (var kvp in keys)
        {
            var contentBytes = Encoding.UTF8.GetBytes(kvp.Value);
            var tempPath = await temporaryCredentialFileStore.SaveAsync(contentBytes, ct);
            tempPaths.Add((kvp.Key, tempPath));
        }

        return Result.Ok<ICredentialContext>(
            new UserCredentialContext(tempPaths, temporaryCredentialFileStore, userContextLogger)
        );
    }

    private async Task<IReadOnlyDictionary<ECredentialType, string>?> ResolveActiveHttpCredentialsAsync(
        CancellationToken ct
    )
    {
        var handle = cookieAccessor.GetCookie();
        if (!IsValidHandle(handle))
        {
            return null;
        }

        var result = await credentialHandleStore.ResolveAsync(handle, ct);
        return result.IsSuccess ? result.Value : null;
    }

    private static bool IsValidHandle(string? handle)
    {
        return handle is { Length: 32 } && handle.All(Uri.IsHexDigit);
    }
}
