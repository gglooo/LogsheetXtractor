namespace WebFormHTR.Application.Interfaces;

public interface ICredentialCookieAccessor
{
    string? GetCookie();
    void SetBackgroundCookie(string cookie);
}