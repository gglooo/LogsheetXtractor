namespace LogsheetXtractor.Application.Features.Credentials;

public static class CredentialProtectionConstants
{
    public const int EnvelopeVersion = 1;

    public static readonly string UserCredentialHandleProtectionPurpose =
        $"LogsheetXtractor.UserOcrCredentials.Handle.v{EnvelopeVersion}";
}
