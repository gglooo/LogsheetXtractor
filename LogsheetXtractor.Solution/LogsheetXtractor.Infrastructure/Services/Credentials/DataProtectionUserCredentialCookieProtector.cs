using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace LogsheetXtractor.Infrastructure.Services.Credentials;

public sealed class DataProtectionUserCredentialCookieProtector(
    IDataProtectionProvider dataProtectionProvider,
    IOptions<UserCredentialCookieOptions> options
) : IUserCredentialCookieProtector
{
    private const string CookiePrefix = "v1:";
    private const string ProtectionPurpose = "LogsheetXtractor.UserOcrCredentials.v1";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly IDataProtector _protector =
        dataProtectionProvider.CreateProtector(ProtectionPurpose);

    public string Protect(IReadOnlyDictionary<ECredentialType, string> keys)
    {
        var normalizedKeys = NormalizeKeys(keys);
        var issuedAtUtc = DateTimeOffset.UtcNow;
        var ttl = options.Value.Ttl > TimeSpan.Zero
            ? options.Value.Ttl
            : TimeSpan.FromDays(365);
        var envelope = new ProtectedCredentialEnvelope(
            1,
            issuedAtUtc.UtcDateTime,
            issuedAtUtc.Add(ttl).UtcDateTime,
            normalizedKeys
        );

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        return CookiePrefix + _protector.Protect(json);
    }

    public Dictionary<ECredentialType, string>? Unprotect(string? protectedCookie)
    {
        if (
            string.IsNullOrWhiteSpace(protectedCookie)
            || !protectedCookie.StartsWith(CookiePrefix, StringComparison.Ordinal)
        )
        {
            return null;
        }

        try
        {
            var protectedPayload = protectedCookie[CookiePrefix.Length..];
            var json = _protector.Unprotect(protectedPayload);
            var envelope = JsonSerializer.Deserialize<ProtectedCredentialEnvelope>(
                json,
                JsonOptions
            );

            if (
                envelope is null
                || envelope.Version != 1
                || envelope.ExpiresAtUtc <= DateTimeOffset.UtcNow
            )
            {
                return null;
            }

            var normalizedKeys = NormalizeKeys(envelope.Keys);
            return normalizedKeys.Count == 0 ? null : normalizedKeys;
        }
        catch (CryptographicException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static Dictionary<ECredentialType, string> NormalizeKeys(
        IReadOnlyDictionary<ECredentialType, string> keys
    )
    {
        return keys
            .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Trim());
    }

    private sealed record ProtectedCredentialEnvelope(
        int Version,
        DateTime IssuedAtUtc,
        DateTime ExpiresAtUtc,
        Dictionary<ECredentialType, string> Keys
    );
}
