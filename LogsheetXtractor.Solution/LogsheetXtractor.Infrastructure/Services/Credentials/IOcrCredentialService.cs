using LogsheetXtractor.Application.Features.Credentials;

namespace LogsheetXtractor.Infrastructure.Services.Credentials;

public interface IOcrCredentialService
{
    (ECredentialType, string)? GetCredentialFilePath(ECredentialType credentialType);
    IEnumerable<(ECredentialType, string)> GetAvailableCredentialsPath();
}
