using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LogsheetXtractor.Infrastructure.Services.Credentials;

public class CredentialCookieAccessor(IHttpContextAccessor httpContextAccessor)
    : ICredentialCookieAccessor
{
    private string? _backgroundCookie;

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

        return _backgroundCookie;
    }

    public void SetBackgroundCookie(string cookie)
    {
        _backgroundCookie = cookie;
    }
}
