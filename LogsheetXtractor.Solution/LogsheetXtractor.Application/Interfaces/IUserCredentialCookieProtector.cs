using LogsheetXtractor.Application.Features.Credentials;

namespace LogsheetXtractor.Application.Interfaces;

public interface IUserCredentialCookieProtector
{
    string Protect(IReadOnlyDictionary<ECredentialType, string> keys);

    Dictionary<ECredentialType, string>? Unprotect(string? protectedCookie);
}
