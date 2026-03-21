using LogsheetXtractor.Application.Features.Credentials;

namespace LogsheetXtractor.Application.Interfaces;

public interface ICredentialService
{
    Task<IEnumerable<ECredentialType>> GetAvailableCredentialTypesAsync(
        CancellationToken cancellationToken
    );
}
