namespace WebFormHTR.Infrastructure.Services.Storage;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(byte[] fileData, string fileName);
    FileStream GetFile(string filePath);
    Task<byte[]> ReadFileAsync(string filePath);
    string GetResolvedPath(string filePath);
    bool DeleteFile(string filePath);
    string ReadAllText(string filePath);
    string GetTemporaryFilePath(string fileName);
    Task<string> SaveTemporaryFileAsync(byte[] fileData, string fileName, CancellationToken ct);
}