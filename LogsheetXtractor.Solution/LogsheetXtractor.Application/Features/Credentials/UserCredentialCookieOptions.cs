namespace LogsheetXtractor.Application.Features.Credentials;

public sealed class UserCredentialCookieOptions
{
    public const string SectionName = "UserCredentialCookie";

    public TimeSpan Ttl { get; set; } = TimeSpan.FromDays(365);
}
