using System.Text;
using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Infrastructure.Services.Storage;
using Microsoft.Extensions.Logging;

namespace LogsheetXtractor.Infrastructure.Services.Credentials;

public class CredentialContextProvider(
    IOcrCredentialService ocrCredentialService,
    IFileStorageService fileStorageService,
    ICredentialCookieAccessor cookieAccessor,
    IUserCredentialCookieProtector credentialCookieProtector,
    ILogger<UserCredentialContext> userContextLogger,
    ILogger<CredentialContextProvider> logger
) : ICredentialContextProvider
{
    public async Task<Result<ICredentialContext>> GetCredentialContextAsync(
        CancellationToken ct = default
    )
    {
        var backgroundError = cookieAccessor.GetBackgroundCredentialError();
        if (!string.IsNullOrWhiteSpace(backgroundError))
        {
            logger.LogWarning(
                "Background user credential snapshot could not be used: {Error}",
                backgroundError
            );
            return Result.Fail<ICredentialContext>(new InvalidStateError(backgroundError));
        }

        var keys = cookieAccessor.GetBackgroundCredentials();

        if (keys is null)
        {
            var cookie = cookieAccessor.GetCookie();
            keys = credentialCookieProtector.Unprotect(cookie);
        }

        if (keys == null)
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
            var fileName = $"{Guid.NewGuid()}_{kvp.Key.ToString().ToLower()}_creds.json";
            var tempPath = await fileStorageService.SaveTemporaryFileAsync(
                contentBytes,
                fileName,
                ct
            );
            tempPaths.Add((kvp.Key, tempPath));
        }

        return Result.Ok<ICredentialContext>(
            new UserCredentialContext(tempPaths, fileStorageService, userContextLogger)
        );
    }
}
