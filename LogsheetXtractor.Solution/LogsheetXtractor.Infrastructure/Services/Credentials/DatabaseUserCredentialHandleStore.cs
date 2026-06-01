using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LogsheetXtractor.Infrastructure.Services.Credentials;

public sealed class DatabaseUserCredentialHandleStore(
    IDataProtectionProvider dataProtectionProvider,
    AppDbContext dbContext,
    ILogger<DatabaseUserCredentialHandleStore> logger
) : IUserCredentialHandleStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector(
        CredentialProtectionConstants.UserCredentialHandleProtectionPurpose
    );

    public async Task<Result<string>> CreateAsync(
        IReadOnlyDictionary<ECredentialType, string> credentials,
        TimeSpan ttl,
        CancellationToken ct = default
    )
    {
        var normalizedCredentials = Normalize(credentials);
        if (normalizedCredentials.Count == 0)
        {
            return Failed();
        }

        var issuedAtUtc = DateTimeOffset.UtcNow;
        var effectiveTtl = ttl > TimeSpan.Zero ? ttl : TimeSpan.FromDays(7);
        var expiresAtUtc = issuedAtUtc.Add(effectiveTtl);
        var envelope = new ProtectedCredentialHandleEnvelope(
            CredentialProtectionConstants.EnvelopeVersion,
            issuedAtUtc,
            expiresAtUtc,
            normalizedCredentials
        );

        var protectedPayload = _protector.Protect(JsonSerializer.Serialize(envelope, JsonOptions));
        var handle = Guid.NewGuid().ToString("N");

        dbContext.UserCredentialHandles.Add(
            new UserCredentialHandle
            {
                Handle = handle,
                ProtectedPayload = protectedPayload,
                IssuedAtUtc = issuedAtUtc.UtcDateTime,
                ExpiresAtUtc = expiresAtUtc.UtcDateTime,
            }
        );

        return await Task.FromResult(Result.Ok(handle));
    }

    public async Task<Result<IReadOnlyDictionary<ECredentialType, string>>> ResolveAsync(
        string? handle,
        CancellationToken ct = default
    )
    {
        if (string.IsNullOrWhiteSpace(handle) || !IsValidHandle(handle))
        {
            return Failed<IReadOnlyDictionary<ECredentialType, string>>();
        }

        var storedHandle = await dbContext.UserCredentialHandles.FirstOrDefaultAsync(
            h => h.Handle == handle,
            ct
        );

        if (storedHandle is null)
        {
            return Failed<IReadOnlyDictionary<ECredentialType, string>>();
        }

        if (storedHandle.ExpiresAtUtc <= DateTime.UtcNow)
        {
            StageDelete(storedHandle);
            return Failed<IReadOnlyDictionary<ECredentialType, string>>();
        }

        try
        {
            var json = _protector.Unprotect(storedHandle.ProtectedPayload);
            var envelope = JsonSerializer.Deserialize<ProtectedCredentialHandleEnvelope>(
                json,
                JsonOptions
            );

            if (
                envelope is null
                || envelope.Version != CredentialProtectionConstants.EnvelopeVersion
                || envelope.ExpiresAtUtc <= DateTimeOffset.UtcNow
            )
            {
                StageDelete(storedHandle);
                return Failed<IReadOnlyDictionary<ECredentialType, string>>();
            }

            var normalizedCredentials = Normalize(envelope.Keys);
            if (normalizedCredentials.Count == 0)
            {
                StageDelete(storedHandle);
                return Failed<IReadOnlyDictionary<ECredentialType, string>>();
            }

            return Result.Ok<IReadOnlyDictionary<ECredentialType, string>>(normalizedCredentials);
        }
        catch (CryptographicException)
        {
            StageDelete(storedHandle);
            return Failed<IReadOnlyDictionary<ECredentialType, string>>();
        }
        catch (JsonException)
        {
            StageDelete(storedHandle);
            return Failed<IReadOnlyDictionary<ECredentialType, string>>();
        }
        catch (ArgumentException)
        {
            StageDelete(storedHandle);
            return Failed<IReadOnlyDictionary<ECredentialType, string>>();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(ex, "Failed to resolve a personal credential handle.");
            return Failed<IReadOnlyDictionary<ECredentialType, string>>();
        }
    }

    public async Task ReleaseAsync(string? handle, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(handle) || !IsValidHandle(handle))
        {
            return;
        }

        var storedHandle = await dbContext.UserCredentialHandles.FirstOrDefaultAsync(
            h => h.Handle == handle,
            ct
        );
        if (storedHandle is not null)
        {
            StageDelete(storedHandle);
        }
    }

    public async Task<int> CleanupExpiredAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await dbContext
            .UserCredentialHandles.Where(h => h.ExpiresAtUtc <= now)
            .ExecuteDeleteAsync(ct);
    }

    private void StageDelete(UserCredentialHandle handle)
    {
        dbContext.UserCredentialHandles.Remove(handle);
    }

    private static bool IsValidHandle(string handle)
    {
        return handle.Length == 32 && handle.All(Uri.IsHexDigit);
    }

    private static Dictionary<ECredentialType, string> Normalize(
        IReadOnlyDictionary<ECredentialType, string> credentials
    )
    {
        return credentials
            .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Trim());
    }

    private static Result<T> Failed<T>()
    {
        return Result.Fail<T>(
            new InvalidStateError(CredentialsConstants.ExpiredBackgroundCredentialHandleMessage)
        );
    }

    private static Result<string> Failed()
    {
        return Result.Fail<string>(
            new InvalidStateError(CredentialsConstants.ExpiredBackgroundCredentialHandleMessage)
        );
    }

    private sealed record ProtectedCredentialHandleEnvelope(
        int Version,
        DateTimeOffset IssuedAtUtc,
        DateTimeOffset ExpiresAtUtc,
        Dictionary<ECredentialType, string> Keys
    );
}
