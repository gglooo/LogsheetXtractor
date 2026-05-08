using System.Text.Json;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LogsheetXtractor.Infrastructure.Services.Credentials;

public class CredentialService(
    IOcrCredentialService ocrCredentialService,
    IHttpContextAccessor httpContextAccessor
) : ICredentialService
{
    public Task<IEnumerable<ECredentialType>> GetAvailableCredentialTypesAsync(
        CancellationToken cancellationToken
    )
    {
        var cookie = httpContextAccessor
            .HttpContext
            ?.Request
            .Cookies[CredentialsConstants.CookieName];
        var keys = CredentialCookieParser.ParseCredentials(cookie);

        if (keys != null)
        {
            return Task.FromResult(keys.Select(k => k.Key));
        }

        // Fallback to server-side credentials if no valid cookie is found
        var availableCredentials = ocrCredentialService
            .GetAvailableCredentialsPath()
            .Select(kvp => kvp.Item1);

        return Task.FromResult(availableCredentials);
    }
}
