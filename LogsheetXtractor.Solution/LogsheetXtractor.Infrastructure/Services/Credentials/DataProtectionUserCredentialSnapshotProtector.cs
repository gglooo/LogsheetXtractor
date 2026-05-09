using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace LogsheetXtractor.Infrastructure.Services.Credentials;

public sealed class DataProtectionUserCredentialSnapshotProtector(
    IDataProtectionProvider dataProtectionProvider,
    IOptions<UserCredentialBackgroundSnapshotOptions> options
) : IUserCredentialSnapshotProtector
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly IDataProtector _protector =
        dataProtectionProvider.CreateProtector(
            CredentialProtectionConstants.BackgroundSnapshotProtectionPurpose
        );

    public string Protect(IReadOnlyDictionary<ECredentialType, string> keys)
    {
        var normalizedKeys = NormalizeKeys(keys);
        var issuedAtUtc = DateTimeOffset.UtcNow;
        var ttl = options.Value.Ttl > TimeSpan.Zero ? options.Value.Ttl : TimeSpan.FromDays(7);
        var envelope = new ProtectedCredentialEnvelope(
            CredentialProtectionConstants.EnvelopeVersion,
            issuedAtUtc,
            issuedAtUtc.Add(ttl),
            normalizedKeys
        );

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        return CredentialProtectionConstants.ProtectedValuePrefix + _protector.Protect(json);
    }

    public Result<Dictionary<ECredentialType, string>> Unprotect(string? protectedSnapshot)
    {
        if (
            string.IsNullOrWhiteSpace(protectedSnapshot)
            || !protectedSnapshot.StartsWith(
                CredentialProtectionConstants.ProtectedValuePrefix,
                StringComparison.Ordinal
            )
        )
        {
            return Failed();
        }

        try
        {
            var protectedPayload =
                protectedSnapshot[CredentialProtectionConstants.ProtectedValuePrefix.Length..];
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
                return Failed();
            }

            var normalizedKeys = NormalizeKeys(envelope.Keys);
            return normalizedKeys.Count == 0 ? Failed() : Result.Ok(normalizedKeys);
        }
        catch (CryptographicException)
        {
            return Failed();
        }
        catch (JsonException)
        {
            return Failed();
        }
        catch (ArgumentException)
        {
            return Failed();
        }
    }

    private static Result<Dictionary<ECredentialType, string>> Failed()
    {
        return Result.Fail<Dictionary<ECredentialType, string>>(
            new InvalidStateError(CredentialsConstants.ExpiredBackgroundSnapshotMessage)
        );
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
