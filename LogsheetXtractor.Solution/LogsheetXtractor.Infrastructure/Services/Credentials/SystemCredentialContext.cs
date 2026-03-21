using LogsheetXtractor.Application.Features.Credentials;

namespace LogsheetXtractor.Infrastructure.Services.Credentials;

public class SystemCredentialContext(IEnumerable<(ECredentialType, string)> credentialPaths)
    : ICredentialContext
{
    public IEnumerable<(ECredentialType, string)> CredentialPaths { get; } = credentialPaths;

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
