namespace LogsheetXtractor.Infrastructure.Persistence.Entities;

public class UserCredentialHandle
{
    public virtual string Handle { get; set; } = string.Empty;

    public virtual string ProtectedPayload { get; set; } = string.Empty;

    public virtual DateTime IssuedAtUtc { get; set; }

    public virtual DateTime ExpiresAtUtc { get; set; }
}
