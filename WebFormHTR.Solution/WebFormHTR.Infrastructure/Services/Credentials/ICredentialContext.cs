using WebFormHTR.Application.Features.Credentials;

namespace WebFormHTR.Infrastructure.Services.Credentials;

public interface ICredentialContext : IAsyncDisposable
{
    IEnumerable<(ECredentialType, string)> CredentialPaths { get; }
}
