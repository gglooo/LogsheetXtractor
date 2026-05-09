using FluentAssertions;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Infrastructure.Services.Credentials;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace LogsheetXtractor.IntegrationTests.Infrastructure.Services.Credentials;

public sealed class DataProtectionUserCredentialCookieProtectorTests : IDisposable
{
    private readonly string _keyDirectory = Path.Combine(
        Path.GetTempPath(),
        $"logsheetxtractor-protector-tests-{Guid.NewGuid():N}"
    );

    [Fact]
    public void ProtectAndUnprotect_ShouldReturnNormalizedCredentials()
    {
        var protector = CreateProtector(TimeSpan.FromDays(365));

        var protectedCookie = protector.Protect(
            new Dictionary<ECredentialType, string>
            {
                [ECredentialType.Google] = " google-key ",
                [ECredentialType.Azure] = "azure-key",
                [ECredentialType.Amazon] = "   ",
            }
        );

        protectedCookie.Should().StartWith(CredentialProtectionConstants.ProtectedValuePrefix);
        protectedCookie.Should().NotContain("google-key");
        protectedCookie.Should().NotContain("azure-key");

        var credentials = protector.Unprotect(protectedCookie);

        credentials.Should().NotBeNull();
        credentials.Should().BeEquivalentTo(
            new Dictionary<ECredentialType, string>
            {
                [ECredentialType.Google] = "google-key",
                [ECredentialType.Azure] = "azure-key",
            }
        );
    }

    [Fact]
    public void Unprotect_ShouldRejectLegacyRawJsonCookie()
    {
        var protector = CreateProtector(TimeSpan.FromDays(365));

        var credentials = protector.Unprotect("{\"Google\":\"google-key\"}");

        credentials.Should().BeNull();
    }

    [Fact]
    public void Unprotect_ShouldRejectTamperedCookie()
    {
        var protector = CreateProtector(TimeSpan.FromDays(365));
        var protectedCookie = protector.Protect(
            new Dictionary<ECredentialType, string> { [ECredentialType.Google] = "google-key" }
        );
        var tamperedCookie = protectedCookie[..^1] + (protectedCookie[^1] == 'A' ? 'B' : 'A');

        var credentials = protector.Unprotect(tamperedCookie);

        credentials.Should().BeNull();
    }

    [Fact]
    public async Task Unprotect_ShouldRejectExpiredCookie()
    {
        var protector = CreateProtector(TimeSpan.FromMilliseconds(10));
        var protectedCookie = protector.Protect(
            new Dictionary<ECredentialType, string> { [ECredentialType.Google] = "google-key" }
        );

        await Task.Delay(TimeSpan.FromMilliseconds(50));

        var credentials = protector.Unprotect(protectedCookie);

        credentials.Should().BeNull();
    }

    [Fact]
    public void ProtectedEnvelope_ShouldUseDateTimeOffsetForExpiration()
    {
        var envelopeType = typeof(DataProtectionUserCredentialCookieProtector).GetNestedType(
            "ProtectedCredentialEnvelope",
            System.Reflection.BindingFlags.NonPublic
        );

        envelopeType
            .Should()
            .NotBeNull()
            .And.Subject!.GetProperty("ExpiresAtUtc")!
            .PropertyType.Should()
            .Be<DateTimeOffset>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_keyDirectory))
        {
            Directory.Delete(_keyDirectory, recursive: true);
        }
    }

    private DataProtectionUserCredentialCookieProtector CreateProtector(TimeSpan ttl)
    {
        Directory.CreateDirectory(_keyDirectory);
        var provider = DataProtectionProvider.Create(new DirectoryInfo(_keyDirectory));
        return new DataProtectionUserCredentialCookieProtector(
            provider,
            Options.Create(new UserCredentialCookieOptions { Ttl = ttl })
        );
    }
}
