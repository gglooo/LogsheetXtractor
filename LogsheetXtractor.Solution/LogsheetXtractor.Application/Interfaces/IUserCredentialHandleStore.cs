using FluentResults;
using LogsheetXtractor.Application.Features.Credentials;

namespace LogsheetXtractor.Application.Interfaces;

public interface IUserCredentialHandleStore
{
    Task<Result<string>> CreateAsync(
        IReadOnlyDictionary<ECredentialType, string> credentials,
        CancellationToken ct = default
    );

    Task<Result<IReadOnlyDictionary<ECredentialType, string>>> ResolveAsync(
        string? handle,
        CancellationToken ct = default
    );

    Task ReleaseAsync(string? handle, CancellationToken ct = default);

    Task<int> CleanupExpiredAsync(CancellationToken ct = default);
}
