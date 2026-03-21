using System.Text;
using System.Text.Json;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Infrastructure.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LogsheetXtractor.Infrastructure.Services.Credentials;

public class CredentialContextProvider(
    IOcrCredentialService ocrCredentialService,
    IFileStorageService fileStorageService,
    ICredentialCookieAccessor cookieAccessor,
    ILogger<UserCredentialContext> userContextLogger,
    ILogger<CredentialContextProvider> logger
) : ICredentialContextProvider
{
    public async Task<ICredentialContext> GetCredentialContextAsync(CancellationToken ct = default)
    {
        var cookie = cookieAccessor.GetCookie();
        var keys = CredentialCookieParser.ParseCredentials(cookie);

        if (keys != null)
        {
            logger.LogInformation(
                "Found active user credentials in cookie. Creating temporary credential files."
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

            return new UserCredentialContext(tempPaths, fileStorageService, userContextLogger);
        }

        logger.LogInformation(
            "No valid user credentials found. Falling back to system credentials."
        );
        return new SystemCredentialContext(ocrCredentialService.GetAvailableCredentialsPath());
    }
}
