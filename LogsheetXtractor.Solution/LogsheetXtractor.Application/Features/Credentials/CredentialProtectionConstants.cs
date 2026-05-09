namespace LogsheetXtractor.Application.Features.Credentials;

public static class CredentialProtectionConstants
{
    public const int EnvelopeVersion = 1;

    public static readonly string ProtectedValuePrefix = $"v{EnvelopeVersion}:";
    public static readonly string CookieProtectionPurpose =
        $"LogsheetXtractor.UserOcrCredentials.v{EnvelopeVersion}";
    public static readonly string BackgroundHandleProtectionPurpose =
        $"LogsheetXtractor.UserOcrCredentials.Handle.v{EnvelopeVersion}";
}
