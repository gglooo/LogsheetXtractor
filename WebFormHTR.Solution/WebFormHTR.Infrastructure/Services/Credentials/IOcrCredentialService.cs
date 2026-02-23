using WebFormHTR.Application.Features.Credentials;

namespace WebFormHTR.Infrastructure.Services.Credentials;

public interface IOcrCredentialService
{
    (ECredentialType, string)? GetCredentialFilePath(ECredentialType credentialType);
    IEnumerable<(ECredentialType, string)> GetAvailableCredentialsPath();
}