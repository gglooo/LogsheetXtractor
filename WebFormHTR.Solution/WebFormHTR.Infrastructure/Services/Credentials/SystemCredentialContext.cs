using WebFormHTR.Application.Features.Credentials;

namespace WebFormHTR.Infrastructure.Services.Credentials;

public class SystemCredentialContext(IEnumerable<(ECredentialType, string)> credentialPaths) : ICredentialContext
{
    public IEnumerable<(ECredentialType, string)> CredentialPaths { get; } = credentialPaths;

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}