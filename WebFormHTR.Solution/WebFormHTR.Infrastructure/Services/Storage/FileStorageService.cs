using Microsoft.Extensions.Configuration;

namespace WebFormHTR.Infrastructure.Services.Storage;

public class FileStorageService(IConfiguration config) : IFileStorageService
{
    private readonly string _storageDirectory =
        Path.GetFullPath(config["Storage:LocalStoragePath"] ?? "app_data/storage");

    private void CheckAndCreateStorageDirectory()
    {
        if (!Directory.Exists(_storageDirectory))
        {
            Directory.CreateDirectory(_storageDirectory);
        }
    }

    public async Task<string> SaveFileAsync(byte[] fileData, string fileName)
    {
        CheckAndCreateStorageDirectory();

        var storedFileName = $"{Guid.NewGuid()}_{fileName}";
        var storagePath = GetResolvedPath(storedFileName);

        await File.WriteAllBytesAsync(storagePath, fileData);

        return storedFileName;
    }

    public FileStream GetFile(string filePath)
    {
        var fullPath = GetResolvedPath(filePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("File not found", fullPath);
        }

        return new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);
    }

    public async Task<byte[]> ReadFileAsync(string filePath)
    {
        var fullPath = GetResolvedPath(filePath);
        return !File.Exists(fullPath)
            ? throw new FileNotFoundException("File not found", fullPath)
            : await File.ReadAllBytesAsync(fullPath);
    }

    public string GetResolvedPath(string filePath)
    {
        if (filePath.StartsWith(_storageDirectory))
        {
            return filePath;
        }

        return Path.Combine(_storageDirectory, filePath);
    }

    public bool DeleteFile(string filePath)
    {
        var fullPath = GetResolvedPath(filePath);
        if (!File.Exists(fullPath))
        {
            return false;
        }

        File.Delete(fullPath);

        return true;
    }

    public string ReadAllText(string filePath)
    {
        var fullPath = GetResolvedPath(filePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("File not found", fullPath);
        }

        return File.ReadAllText(fullPath);
    }

    public string GetTemporaryFilePath(string fileName)
    {
        return Path.Combine(Path.GetTempPath(), fileName);
    }

    public async Task<string> SaveTemporaryFileAsync(byte[] fileData, string fileName, CancellationToken ct)
    {
        var tempFilePath = GetTemporaryFilePath(fileName);
        await File.WriteAllBytesAsync(tempFilePath, fileData, ct);

        return tempFilePath;
    }

    public FileStream GetTemporaryFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Temporary file not found", filePath);
        }

        return new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);
    }

    public Task<byte[]> ReadTemporaryFileAsync(string filePath, CancellationToken ct)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Temporary file not found", filePath);
        }

        return File.ReadAllBytesAsync(filePath, ct);
    }
}