namespace LogsheetXtractor.Infrastructure.Services.Credentials;

public interface ICredentialContextProvider
{
    Task<ICredentialContext> GetCredentialContextAsync(CancellationToken ct = default);
}
