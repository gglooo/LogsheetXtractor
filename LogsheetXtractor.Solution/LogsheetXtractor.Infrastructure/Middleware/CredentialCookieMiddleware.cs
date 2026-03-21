using LogsheetXtractor.Application.Interfaces;
using Wolverine;

namespace LogsheetXtractor.Infrastructure.Middleware;

public static class CredentialCookieMiddleware
{
    public static void Before(Envelope envelope, ICredentialCookieAccessor cookieAccessor)
    {
        if (envelope.Headers.TryGetValue("UserCookie", out var cookie) && cookie != null)
        {
            cookieAccessor.SetBackgroundCookie(cookie);
        }
    }
}
