using WebFormHTR.Application.Features.Credentials;

namespace WebFormHTR.Application.Interfaces;

public interface ICredentialService
{
    Task<IEnumerable<ECredentialType>> GetAvailableCredentialTypesAsync(CancellationToken cancellationToken);
}