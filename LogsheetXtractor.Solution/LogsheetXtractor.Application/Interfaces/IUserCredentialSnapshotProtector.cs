using FluentResults;
using LogsheetXtractor.Application.Features.Credentials;

namespace LogsheetXtractor.Application.Interfaces;

public interface IUserCredentialSnapshotProtector
{
    string Protect(IReadOnlyDictionary<ECredentialType, string> keys);

    Result<Dictionary<ECredentialType, string>> Unprotect(string? protectedSnapshot);
}
