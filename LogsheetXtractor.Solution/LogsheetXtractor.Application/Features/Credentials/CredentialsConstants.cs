namespace LogsheetXtractor.Application.Features.Credentials;

public static class CredentialsConstants
{
    public const string CookieName = "UserOcrCredentials";
    public const string BackgroundSnapshotHeaderName = "UserCredentialSnapshot";
    public const string ExpiredBackgroundSnapshotMessage =
        "Personal OCR credentials expired. Re-run processing after setting credentials again.";
}
