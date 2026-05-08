using LogsheetXtractor.Application.Features.Credentials;

namespace LogsheetXtractor.Application.Interfaces;

public interface ICredentialCookieAccessor
{
    string? GetCookie();
    IReadOnlyDictionary<ECredentialType, string>? GetBackgroundCredentials();
    string? GetBackgroundCredentialError();
    void SetBackgroundCredentials(IReadOnlyDictionary<ECredentialType, string> credentials);
    void SetBackgroundCredentialError(string errorMessage);
}
