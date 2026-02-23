using Microsoft.Extensions.Configuration;
using WebFormHTR.Application.Features.Credentials;

namespace WebFormHTR.Infrastructure.Services.Credentials;

public class OcrCredentialService(IConfiguration config) : IOcrCredentialService
{
    private readonly string _googleCredentialsPath = config["Credentials:GoogleApiKeyPath"] ?? string.Empty;
    private readonly string _azureCredentialsPath = config["Credentials:AzureApiKeyPath"] ?? string.Empty;
    private readonly string _amazonCredentialsPath = config["Credentials:AmazonApiKeyPath"] ?? string.Empty;

    private Dictionary<ECredentialType, string> CredentialPaths => new()
    {
        { ECredentialType.Google, _googleCredentialsPath },
        { ECredentialType.Azure, _azureCredentialsPath },
        { ECredentialType.Amazon, _amazonCredentialsPath }
    };

    public (ECredentialType, string)? GetCredentialFilePath(ECredentialType credentialType)
    {
        if (!CredentialPaths.TryGetValue(credentialType, out var path) || string.IsNullOrEmpty(path))
        {
            return null;
        }

        var fullPath = Path.GetFullPath(path);
        return Path.Exists(path) ? (credentialType, path) : null;
    }

    public IEnumerable<(ECredentialType, string)> GetAvailableCredentialsPath()
    {
        foreach (var kvp in CredentialPaths)
        {
            if (GetCredentialFilePath(kvp.Key) is not null)
            {
                yield return (kvp.Key, kvp.Value);
            }
        }
    }
}