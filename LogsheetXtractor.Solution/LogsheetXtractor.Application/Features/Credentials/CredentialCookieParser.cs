using System.Text.Json;

namespace LogsheetXtractor.Application.Features.Credentials;

public static class CredentialCookieParser
{
    public static Dictionary<ECredentialType, string>? ParseCredentials(string? cookieString)
    {
        if (string.IsNullOrEmpty(cookieString))
        {
            return null;
        }

        try
        {
            var keys = JsonSerializer.Deserialize<Dictionary<ECredentialType, string>>(cookieString);

            if (keys != null && keys.Any(k => !string.IsNullOrWhiteSpace(k.Value)))
            {
                var validKeys = keys
                    .Where(k => !string.IsNullOrWhiteSpace(k.Value))
                    .ToDictionary(k => k.Key, k => k.Value);

                if (validKeys.Count != 0)
                {
                    return validKeys;
                }
            }
        }
        catch (JsonException)
        {
        }

        return null;
    }
}