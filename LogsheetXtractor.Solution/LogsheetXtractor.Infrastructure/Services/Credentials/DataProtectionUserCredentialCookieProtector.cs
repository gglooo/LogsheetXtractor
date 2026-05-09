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
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly IDataProtector _protector =
        dataProtectionProvider.CreateProtector(CredentialProtectionConstants.CookieProtectionPurpose);

    public string Protect(IReadOnlyDictionary<ECredentialType, string> keys)
    {
        var normalizedKeys = NormalizeKeys(keys);
        var issuedAtUtc = DateTimeOffset.UtcNow;
        var ttl = options.Value.Ttl > TimeSpan.Zero
            ? options.Value.Ttl
            : TimeSpan.FromDays(365);
        var envelope = new ProtectedCredentialEnvelope(
            CredentialProtectionConstants.EnvelopeVersion,
            issuedAtUtc,
            issuedAtUtc.Add(ttl),
            normalizedKeys
        );

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        return CredentialProtectionConstants.ProtectedValuePrefix + _protector.Protect(json);
    }

    public Dictionary<ECredentialType, string>? Unprotect(string? protectedCookie)
    {
        if (
            string.IsNullOrWhiteSpace(protectedCookie)
            || !protectedCookie.StartsWith(
                CredentialProtectionConstants.ProtectedValuePrefix,
                StringComparison.Ordinal
            )
        )
        {
            return null;
        }

        try
        {
            var protectedPayload =
                protectedCookie[CredentialProtectionConstants.ProtectedValuePrefix.Length..];
            var json = _protector.Unprotect(protectedPayload);
            var envelope = JsonSerializer.Deserialize<ProtectedCredentialEnvelope>(
                json,
                JsonOptions
            );

            if (
                envelope is null
                || envelope.Version != CredentialProtectionConstants.EnvelopeVersion
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
        DateTimeOffset IssuedAtUtc,
        DateTimeOffset ExpiresAtUtc,
        Dictionary<ECredentialType, string> Keys
    );
}
