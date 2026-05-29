using System.Text;
using FluentAssertions;
using LogsheetXtractor.Infrastructure.Services.Credentials;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LogsheetXtractor.IntegrationTests.Infrastructure.Services.Credentials;

public sealed class TemporaryCredentialFileStoreTests : IDisposable
{
    private readonly string _storageDirectory;
    private readonly TemporaryCredentialFileStore _store;

    public TemporaryCredentialFileStoreTests()
    {
        _storageDirectory = Path.Combine(
            Path.GetTempPath(),
            $"TemporaryCredentialFileStoreTests_{Guid.NewGuid()}"
        );

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    { "Storage:LocalStoragePath", _storageDirectory },
                }
            )
            .Build();

        _store = new TemporaryCredentialFileStore(
            config,
            NullLogger<TemporaryCredentialFileStore>.Instance
        );
    }

    [Fact]
    public async Task SaveAsync_ShouldCreateRandomCredentialFileInDedicatedDirectory()
    {
        var content = Encoding.UTF8.GetBytes("secret-value");

        var path = await _store.SaveAsync(content);

        path.Should().StartWith(
            Path.Combine(_storageDirectory, "tmp", "credentials") + Path.DirectorySeparatorChar
        );
        Path.GetFileName(path).Should().EndWith(".json");
        Path.GetFileName(path).Should().NotContain("google");
        Path.GetFileName(path).Should().NotContain("azure");
        Path.GetFileName(path).Should().NotContain("amazon");
        File.Exists(path).Should().BeTrue();
        var savedContent = await File.ReadAllTextAsync(path);
        savedContent.Should().Be("secret-value");
    }

    [Fact]
    public async Task Delete_ShouldDeleteCredentialTempFile()
    {
        var path = await _store.SaveAsync(Encoding.UTF8.GetBytes("secret-value"));

        var deleted = _store.Delete(path);

        deleted.Should().BeTrue();
        File.Exists(path).Should().BeFalse();
    }

    [Fact]
    public void Delete_ShouldNotDeleteFilesOutsideCredentialTempDirectory()
    {
        Directory.CreateDirectory(_storageDirectory);
        var path = Path.Combine(_storageDirectory, "other.json");
        File.WriteAllText(path, "secret-value");

        var deleted = _store.Delete(path);

        deleted.Should().BeFalse();
        File.Exists(path).Should().BeTrue();
    }

    [Fact]
    public async Task CleanupStaleFiles_ShouldDeleteOnlyOldCredentialFiles()
    {
        var oldPath = await _store.SaveAsync(Encoding.UTF8.GetBytes("old-secret"));
        var freshPath = await _store.SaveAsync(Encoding.UTF8.GetBytes("fresh-secret"));
        File.SetLastWriteTimeUtc(oldPath, DateTime.UtcNow.AddHours(-25));

        var deletedCount = _store.CleanupStaleFiles(TimeSpan.FromHours(24));

        deletedCount.Should().Be(1);
        File.Exists(oldPath).Should().BeFalse();
        File.Exists(freshPath).Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_ShouldApplyOwnerOnlyPermissions_WhenUnixPermissionsAreSupported()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var path = await _store.SaveAsync(Encoding.UTF8.GetBytes("secret-value"));

        File.GetUnixFileMode(path).Should().Be(UnixFileMode.UserRead | UnixFileMode.UserWrite);
        File.GetUnixFileMode(Path.GetDirectoryName(path)!)
            .Should()
            .Be(UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
    }

    [Fact]
    public async Task SaveAsync_ShouldNotMakeCredentialFileReadableOutsideOwner_WhenUnixPermissionsAreSupported()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var path = await _store.SaveAsync(Encoding.UTF8.GetBytes("secret-value"));

        var fileMode = File.GetUnixFileMode(path);
        fileMode.Should().NotHaveFlag(UnixFileMode.GroupRead);
        fileMode.Should().NotHaveFlag(UnixFileMode.OtherRead);

        var directoryMode = File.GetUnixFileMode(Path.GetDirectoryName(path)!);
        directoryMode.Should().NotHaveFlag(UnixFileMode.GroupExecute);
        directoryMode.Should().NotHaveFlag(UnixFileMode.OtherExecute);
    }

    public void Dispose()
    {
        if (Directory.Exists(_storageDirectory))
        {
            Directory.Delete(_storageDirectory, true);
        }
    }
}
