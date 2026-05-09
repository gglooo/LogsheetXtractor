using FluentResults;

namespace LogsheetXtractor.Infrastructure.Services.Credentials;

public interface ICredentialContextProvider
{
    Task<Result<ICredentialContext>> GetCredentialContextAsync(CancellationToken ct = default);
}
