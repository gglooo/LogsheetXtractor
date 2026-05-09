using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LogsheetXtractor.Infrastructure.Services.Credentials;

public class CredentialService(
    IOcrCredentialService ocrCredentialService
) : ICredentialService
{
    public Task<IEnumerable<ECredentialType>> GetAvailableCredentialTypesAsync(
        CancellationToken cancellationToken
    )
    {
        var availableCredentials = ocrCredentialService
            .GetAvailableCredentialsPath()
            .Select(kvp => kvp.Item1);

        return Task.FromResult(availableCredentials);
    }
}
