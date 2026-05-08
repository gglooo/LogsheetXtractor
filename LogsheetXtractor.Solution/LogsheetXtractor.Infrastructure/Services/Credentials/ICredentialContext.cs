using LogsheetXtractor.Application.Features.Credentials;

namespace LogsheetXtractor.Infrastructure.Services.Credentials;

public interface ICredentialContext : IAsyncDisposable
{
    IEnumerable<(ECredentialType, string)> CredentialPaths { get; }
}
