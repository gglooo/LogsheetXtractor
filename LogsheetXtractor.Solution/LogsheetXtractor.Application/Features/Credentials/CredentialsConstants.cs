namespace LogsheetXtractor.Application.Features.Credentials;

public static class CredentialsConstants
{
    public const string CookieName = "UserOcrCredentials";
    public const string BackgroundHandleHeaderName = "UserCredentialHandle";
    public const string ExpiredBackgroundCredentialHandleMessage =
        "Personal OCR credentials expired. Re-run processing after setting credentials again.";
}
