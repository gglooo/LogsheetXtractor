using FluentAssertions;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Infrastructure.Services.Credentials;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace LogsheetXtractor.IntegrationTests.Infrastructure.Services.Credentials;

public sealed class DataProtectionUserCredentialSnapshotProtectorTests : IDisposable
{
    private readonly string _keyDirectory = Path.Combine(
        Path.GetTempPath(),
        $"logsheetxtractor-snapshot-protector-tests-{Guid.NewGuid():N}"
    );

    [Fact]
    public void ProtectAndUnprotect_ShouldReturnNormalizedCredentials()
    {
        var protector = CreateProtector(TimeSpan.FromDays(7));

        var protectedSnapshot = protector.Protect(
            new Dictionary<ECredentialType, string>
            {
                [ECredentialType.Google] = " google-key ",
                [ECredentialType.Azure] = "azure-key",
                [ECredentialType.Amazon] = "   ",
            }
        );

        protectedSnapshot.Should().StartWith(CredentialProtectionConstants.ProtectedValuePrefix);
        protectedSnapshot.Should().NotContain("google-key");
        protectedSnapshot.Should().NotContain("azure-key");

        var result = protector.Unprotect(protectedSnapshot);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(
            new Dictionary<ECredentialType, string>
            {
                [ECredentialType.Google] = "google-key",
                [ECredentialType.Azure] = "azure-key",
            }
        );
    }

    [Fact]
    public void Unprotect_ShouldRejectTamperedSnapshot()
    {
        var protector = CreateProtector(TimeSpan.FromDays(7));
        var protectedSnapshot = protector.Protect(
            new Dictionary<ECredentialType, string> { [ECredentialType.Google] = "google-key" }
        );
        var tamperedSnapshot =
            protectedSnapshot[..^1] + (protectedSnapshot[^1] == 'A' ? 'B' : 'A');

        var result = protector.Unprotect(tamperedSnapshot);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.Message == CredentialsConstants.ExpiredBackgroundSnapshotMessage
        );
    }

    [Fact]
    public async Task Unprotect_ShouldRejectExpiredSnapshot()
    {
        var protector = CreateProtector(TimeSpan.FromMilliseconds(10));
        var protectedSnapshot = protector.Protect(
            new Dictionary<ECredentialType, string> { [ECredentialType.Google] = "google-key" }
        );

        await Task.Delay(TimeSpan.FromMilliseconds(50));

        var result = protector.Unprotect(protectedSnapshot);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.Message == CredentialsConstants.ExpiredBackgroundSnapshotMessage
        );
    }

    public void Dispose()
    {
        if (Directory.Exists(_keyDirectory))
        {
            Directory.Delete(_keyDirectory, recursive: true);
        }
    }

    private DataProtectionUserCredentialSnapshotProtector CreateProtector(TimeSpan ttl)
    {
        Directory.CreateDirectory(_keyDirectory);
        var provider = DataProtectionProvider.Create(new DirectoryInfo(_keyDirectory));
        return new DataProtectionUserCredentialSnapshotProtector(
            provider,
            Options.Create(new UserCredentialBackgroundSnapshotOptions { Ttl = ttl })
        );
    }
}
