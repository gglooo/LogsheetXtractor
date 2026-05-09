namespace LogsheetXtractor.Application.Features.Credentials;

public sealed class UserCredentialCookieOptions
{
    public const string SectionName = "UserCredentialCookie";

    public TimeSpan Ttl { get; set; } = TimeSpan.FromDays(365);
}

public sealed class UserCredentialBackgroundHandleOptions
{
    public const string SectionName = "UserCredentialBackgroundHandle";

    public TimeSpan Ttl { get; set; } = TimeSpan.FromDays(7);
}
