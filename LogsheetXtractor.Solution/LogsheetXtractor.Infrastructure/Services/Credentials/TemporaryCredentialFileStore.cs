using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogsheetXtractor.Infrastructure.Services.Credentials;

public sealed class TemporaryCredentialFileStore(
    IConfiguration config,
    ILogger<TemporaryCredentialFileStore> logger
) : ITemporaryCredentialFileStore
{
    private static readonly UnixFileMode OwnerOnlyDirectoryMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;

    private static readonly UnixFileMode OwnerOnlyFileMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite;

    private readonly string _credentialDirectory = Path.GetFullPath(
        Path.Combine(
            config["Storage:LocalStoragePath"] ?? "app_data/storage",
            "tmp",
            "credentials"
        )
    );

    public void EnsureDirectory()
    {
        var parentDirectory = Path.GetDirectoryName(_credentialDirectory);
        if (!string.IsNullOrWhiteSpace(parentDirectory))
        {
            Directory.CreateDirectory(parentDirectory);
            TrySetDirectoryPermissions(parentDirectory);
        }

        Directory.CreateDirectory(_credentialDirectory);
        TrySetDirectoryPermissions(_credentialDirectory);
    }

    public async Task<string> SaveAsync(byte[] fileData, CancellationToken ct = default)
    {
        EnsureDirectory();

        var filePath = Path.Combine(_credentialDirectory, $"{Guid.NewGuid():N}.json");
        var options = new FileStreamOptions
        {
            Mode = FileMode.CreateNew,
            Access = FileAccess.Write,
            Share = FileShare.None,
            Options = FileOptions.WriteThrough,
        };

        if (!OperatingSystem.IsWindows())
        {
            options.UnixCreateMode = OwnerOnlyFileMode;
        }

        await using (var stream = new FileStream(filePath, options))
        {
            await stream.WriteAsync(fileData, ct);
        }

        TrySetFilePermissions(filePath);
        return filePath;
    }

    public bool Delete(string filePath)
    {
        if (!IsCredentialTempFile(filePath))
        {
            logger.LogWarning(
                "Refusing to delete temporary personal credential file outside the credential temp directory."
            );
            return false;
        }

        if (!File.Exists(filePath))
        {
            return false;
        }

        File.Delete(filePath);
        return true;
    }

    public int CleanupStaleFiles(TimeSpan minimumAge)
    {
        EnsureDirectory();

        var cutoffUtc = DateTimeOffset.UtcNow - minimumAge;
        var deletedCount = 0;

        foreach (var filePath in Directory.EnumerateFiles(_credentialDirectory, "*.json"))
        {
            try
            {
                if (File.GetLastWriteTimeUtc(filePath) > cutoffUtc.UtcDateTime)
                {
                    continue;
                }

                File.Delete(filePath);
                deletedCount++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to delete a stale temporary personal credential file."
                );
            }
        }

        return deletedCount;
    }

    private bool IsCredentialTempFile(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);
        var directory = _credentialDirectory.TrimEnd(Path.DirectorySeparatorChar);
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        return fullPath.StartsWith(
            directory + Path.DirectorySeparatorChar,
            comparison
        );
    }

    private void TrySetDirectoryPermissions(string directoryPath)
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        try
        {
            File.SetUnixFileMode(directoryPath, OwnerOnlyDirectoryMode);
        }
        catch (Exception ex)
        {
            logger.LogDebug(
                ex,
                "Could not apply owner-only permissions to the credential temp directory."
            );
        }
    }

    private void TrySetFilePermissions(string filePath)
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        try
        {
            File.SetUnixFileMode(filePath, OwnerOnlyFileMode);
        }
        catch (Exception ex)
        {
            logger.LogDebug(
                ex,
                "Could not apply owner-only permissions to a temporary personal credential file."
            );
        }
    }
}
