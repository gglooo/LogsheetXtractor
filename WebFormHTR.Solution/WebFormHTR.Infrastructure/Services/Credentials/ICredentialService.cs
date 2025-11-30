namespace WebFormHTR.Infrastructure.Services.Credentials;

public interface ICredentialService
{
    (ECredentialType, string)? GetCredentialFilePath(ECredentialType credentialType);
    IEnumerable<(ECredentialType, string)> GetAvailableCredentialsPath();
}