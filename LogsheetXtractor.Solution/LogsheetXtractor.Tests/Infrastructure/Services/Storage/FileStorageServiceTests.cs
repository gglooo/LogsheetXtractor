using FluentAssertions;
using LogsheetXtractor.Infrastructure.Services.Storage;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LogsheetXtractor.Tests.Infrastructure.Services.Storage;

public class FileStorageServiceTests : IDisposable
{
    private readonly string _storageDirectory;
    private readonly FileStorageService _fileStorageService;

    public FileStorageServiceTests()
    {
        _storageDirectory = Path.Combine(Path.GetTempPath(), $"FileStorageTests_{Guid.NewGuid()}");

        var configMock = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    { "Storage:LocalStoragePath", _storageDirectory },
                }
            )
            .Build();

        _fileStorageService = new FileStorageService(configMock);
    }

    [Fact]
    public async Task SaveFileAsync_ShouldCreateFile()
    {
        var content = new byte[] { 1, 2, 3 };
        var fileName = "test.txt";

        var storedFileName = await _fileStorageService.SaveFileAsync(content, fileName);

        storedFileName.Should().Contain(fileName);

        var fullPath = Path.Combine(_storageDirectory, storedFileName);
        File.Exists(fullPath).Should().BeTrue();

        var savedContent = await File.ReadAllBytesAsync(fullPath);
        savedContent.Should().BeEquivalentTo(content);
    }

    [Fact]
    public async Task ReadFileAsync_ShouldReturnContent()
    {
        var content = new byte[] { 4, 5, 6 };
        var fileName = "read.txt";
        var storedFileName = await _fileStorageService.SaveFileAsync(content, fileName);

        var readContent = await _fileStorageService.ReadFileAsync(storedFileName);

        readContent.Should().BeEquivalentTo(content);
    }

    [Fact]
    public async Task ReadFileAsync_ShouldThrow_WhenFileNotFound()
    {
        var act = async () => await _fileStorageService.ReadFileAsync("nonexistent.txt");

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task GetFile_ShouldReturnStream()
    {
        var content = new byte[] { 7, 8, 9 };
        var fileName = "stream.txt";
        var storedFileName = await _fileStorageService.SaveFileAsync(content, fileName);

        await using var stream = _fileStorageService.GetFile(storedFileName);

        stream.Should().NotBeNull();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        ms.ToArray().Should().BeEquivalentTo(content);
    }

    [Fact]
    public void GetFile_ShouldThrow_WhenFileNotFound()
    {
        var act = () => _fileStorageService.GetFile("nonexistent.txt");

        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public async Task DeleteFile_ShouldRemoveFile()
    {
        var content = new byte[] { 10 };
        var fileName = "delete.txt";
        var storedFileName = await _fileStorageService.SaveFileAsync(content, fileName);

        var result = _fileStorageService.DeleteFile(storedFileName);

        result.Should().BeTrue();

        var fullPath = Path.Combine(_storageDirectory, storedFileName);
        File.Exists(fullPath).Should().BeFalse();
    }

    [Fact]
    public void DeleteFile_ShouldReturnFalse_WhenFileNotFound()
    {
        var result = _fileStorageService.DeleteFile("nonexistent.txt");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReadAllText_ShouldReturnString()
    {
        var contentString = "Hello World";
        var content = System.Text.Encoding.UTF8.GetBytes(contentString);
        var fileName = "text.txt";
        var storedFileName = await _fileStorageService.SaveFileAsync(content, fileName);

        var readString = await _fileStorageService.ReadAllTextAsync(storedFileName);

        readString.Should().Be(contentString);
    }

    public void Dispose()
    {
        if (Directory.Exists(_storageDirectory))
        {
            try
            {
                Directory.Delete(_storageDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
