using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LogsheetXtractor.Infrastructure.Services.Credentials;

public class CredentialCookieAccessor(IHttpContextAccessor httpContextAccessor)
    : ICredentialCookieAccessor
{
    private IReadOnlyDictionary<ECredentialType, string>? _backgroundCredentials;
    private string? _backgroundCredentialError;

    public string? GetCookie()
    {
        var httpCookie = httpContextAccessor
            .HttpContext
            ?.Request
            .Cookies[CredentialsConstants.CookieName];
        if (!string.IsNullOrEmpty(httpCookie))
        {
            return httpCookie;
        }

        return null;
    }

    public IReadOnlyDictionary<ECredentialType, string>? GetBackgroundCredentials()
    {
        return _backgroundCredentials;
    }

    public string? GetBackgroundCredentialError()
    {
        return _backgroundCredentialError;
    }

    public void SetBackgroundCredentials(IReadOnlyDictionary<ECredentialType, string> credentials)
    {
        _backgroundCredentials = credentials;
        _backgroundCredentialError = null;
    }

    public void SetBackgroundCredentialError(string errorMessage)
    {
        _backgroundCredentials = null;
        _backgroundCredentialError = errorMessage;
    }
}
