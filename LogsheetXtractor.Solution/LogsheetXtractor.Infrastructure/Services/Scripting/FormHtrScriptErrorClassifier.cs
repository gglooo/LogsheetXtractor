using LogsheetXtractor.Application.Features.Scripting;

namespace LogsheetXtractor.Infrastructure.Services.Scripting;

public sealed class FormHtrScriptErrorClassifier : IScriptErrorClassifier
{
    public ScriptFailureKind ClassifyProcessLogsheetFailure(string rawError)
    {
        if (string.IsNullOrWhiteSpace(rawError))
        {
            return ScriptFailureKind.Unknown;
        }

        var normalizedError = rawError.ToLowerInvariant();

        return IsGoogleCredentialFailure(normalizedError)
            ? ScriptFailureKind.OcrCredentialsInvalid
            : ScriptFailureKind.Unknown;
    }

    private static bool IsGoogleCredentialFailure(string normalizedError)
    {
        var mentionsGoogleCredentials =
            normalizedError.Contains("service_account")
            || normalizedError.Contains("from_service_account_file")
            || normalizedError.Contains("google/auth/_service_account_info.py")
            || normalizedError.Contains("google.oauth2.service_account")
            || normalizedError.Contains("google.api_core.exceptions.unauthenticated")
            || normalizedError.Contains("google.api_core.exceptions.permissiondenied");

        var mentionsCredentialError =
            normalizedError.Contains("json.decoder.jsondecodeerror")
            || normalizedError.Contains("invalid_grant")
            || normalizedError.Contains("unauthenticated")
            || normalizedError.Contains("permission denied")
            || normalizedError.Contains("permissiondenied")
            || normalizedError.Contains("permission_denied")
            || normalizedError.Contains("invalid api key")
            || normalizedError.Contains("api key not valid")
            || normalizedError.Contains("malformederror")
            || normalizedError.Contains("service account info was not in the expected format")
            || normalizedError.Contains("could not automatically determine credentials");

        return mentionsGoogleCredentials && mentionsCredentialError;
    }
}
