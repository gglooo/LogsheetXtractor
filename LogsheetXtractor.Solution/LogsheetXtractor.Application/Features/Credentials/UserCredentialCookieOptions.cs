namespace LogsheetXtractor.Application.Features.Credentials;

public sealed class UserCredentialCookieOptions
{
    public const string SectionName = "UserCredentialCookie";

    public TimeSpan Ttl { get; set; } = TimeSpan.FromDays(365);
}

public sealed class UserCredentialBackgroundSnapshotOptions
{
    public const string SectionName = "UserCredentialBackgroundSnapshot";

    public TimeSpan Ttl { get; set; } = TimeSpan.FromDays(7);
}
