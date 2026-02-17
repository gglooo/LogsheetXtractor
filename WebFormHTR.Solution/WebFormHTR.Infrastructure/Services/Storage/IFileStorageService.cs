namespace WebFormHTR.Infrastructure.Services.Storage;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(byte[] fileData, string fileName);
    FileStream GetFile(string filePath);
    Task<byte[]> ReadFileAsync(string filePath);
    string GetResolvedPath(string filePath);
    bool DeleteFile(string filePath);
    Task<string> ReadAllTextAsync(string filePath, CancellationToken ct = default);
    string GetTemporaryFilePath(string fileName);
    Task<string> SaveTemporaryFileAsync(byte[] fileData, string fileName, CancellationToken ct);
    FileStream GetTemporaryFile(string filePath);
    Task<byte[]> ReadTemporaryFileAsync(string filePath, CancellationToken ct);
}