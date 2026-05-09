namespace LogsheetXtractor.Infrastructure.Services.Credentials;

public interface ITemporaryCredentialFileStore
{
    Task<string> SaveAsync(byte[] fileData, CancellationToken ct = default);
    bool Delete(string filePath);
    int CleanupStaleFiles(TimeSpan minimumAge);
    void EnsureDirectory();
}
