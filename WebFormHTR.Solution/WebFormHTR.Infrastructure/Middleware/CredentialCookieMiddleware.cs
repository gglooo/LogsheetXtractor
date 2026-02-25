using WebFormHTR.Application.Interfaces;
using Wolverine;

namespace WebFormHTR.Infrastructure.Middleware;

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