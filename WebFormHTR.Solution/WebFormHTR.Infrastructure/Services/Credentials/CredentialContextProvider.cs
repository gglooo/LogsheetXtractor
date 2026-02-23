using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WebFormHTR.Application.Features.Credentials;
using WebFormHTR.Infrastructure.Services.Storage;

namespace WebFormHTR.Infrastructure.Services.Credentials;

public class CredentialContextProvider(
    IOcrCredentialService ocrCredentialService,
    IFileStorageService fileStorageService,
    IHttpContextAccessor httpContextAccessor,
    ILogger<UserCredentialContext> userContextLogger,
    ILogger<CredentialContextProvider> logger) : ICredentialContextProvider
{
    public async Task<ICredentialContext> GetCredentialContextAsync(CancellationToken ct = default)
    {
        var cookie = httpContextAccessor.HttpContext?.Request.Cookies[CredentialsConstants.CookieName];
        var keys = CredentialCookieParser.ParseCredentials(cookie);

        if (keys != null)
        {
            logger.LogInformation("Found active user credentials in cookie. Creating temporary credential files.");
            var tempPaths = new List<(ECredentialType, string)>();

            foreach (var kvp in keys)
            {
                var contentBytes = Encoding.UTF8.GetBytes(kvp.Value);
                var fileName = $"{Guid.NewGuid()}_{kvp.Key.ToString().ToLower()}_creds.json";
                var tempPath = await fileStorageService.SaveTemporaryFileAsync(contentBytes, fileName, ct);
                tempPaths.Add((kvp.Key, tempPath));
            }

            return new UserCredentialContext(tempPaths, fileStorageService, userContextLogger);
        }

        logger.LogInformation("No valid user credentials found. Falling back to system credentials.");
        return new SystemCredentialContext(ocrCredentialService.GetAvailableCredentialsPath());
    }
}