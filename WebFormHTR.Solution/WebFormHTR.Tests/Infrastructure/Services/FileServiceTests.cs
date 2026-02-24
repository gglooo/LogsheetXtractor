using FluentAssertions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Infrastructure.Services;
using Xunit;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Infrastructure.Services.Storage;
using WebFormHTR.Tests.Common;

namespace WebFormHTR.Tests.Infrastructure.Services;

using Docnet.Core;

public class FileServiceTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<IDocLib> _docLibMock;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<FileService>> _loggerMock;
    private readonly FileService _fileService;
    private readonly IMemoryCache _cache;

    public FileServiceTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mapperMock = new Mock<IMapper>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _docLibMock = new Mock<IDocLib>();
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<FileService>>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _fileService = new FileService(_dbContext, _mapperMock.Object, _fileStorageServiceMock.Object,
            _docLibMock.Object, _loggerMock.Object, _cache);
    }

    [Fact]
    public async Task UploadFileAsync_ShouldSaveFileToDiskAndDb()
    {
        var content = new byte[] { 1, 2, 3 };
        var fileName = "test.txt";
        var contentType = "text/plain";
        var expectedDto = new FileDto(Guid.NewGuid(), fileName, contentType, (uint)content.Length, DateTime.UtcNow);
        var storagePath = "some/path/test.txt";

        _mapperMock.Setup(x => x.Map<FileDto>(It.IsAny<WebFormHTR.Domain.Entities.File>()))
            .Returns(expectedDto);

        _fileStorageServiceMock.Setup(x => x.SaveFileAsync(content, fileName))
            .ReturnsAsync(storagePath);

        var result = await _fileService.UploadFileAsync(content, fileName, contentType);
        await _dbContext.SaveChangesAsync();

        result.Should().Be(expectedDto);

        var savedFile = await _dbContext.Files.FirstOrDefaultAsync();
        savedFile.Should().NotBeNull();
        savedFile!.OriginalFileName.Should().Be(fileName);
        savedFile.StoragePath.Should().Be(storagePath);

        _fileStorageServiceMock.Verify(x => x.SaveFileAsync(content, fileName), Times.Once);
    }

    [Fact]
    public async Task GetFileAsync_ShouldReturnFile_WhenExists()
    {
        var content = new byte[] { 4, 5, 6 };
        var fileName = "existing.txt";
        var contentType = "text/plain";
        var storedFileName = $"{Guid.NewGuid()}_{fileName}";
        var storagePath = $"FileStorage/{storedFileName}";

        var fileEntity = new WebFormHTR.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            OriginalFileName = fileName,
            StoredFileName = storedFileName,
            StoragePath = storagePath,
            ContentType = contentType,
            SizeBytes = (uint)content.Length
        };
        _dbContext.Files.Add(fileEntity);
        await _dbContext.SaveChangesAsync();

        var tempFile = Path.GetTempFileName();
        await File.WriteAllBytesAsync(tempFile, content);
        var fileStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
            FileOptions.DeleteOnClose);

        _fileStorageServiceMock.Setup(x => x.GetFile(storagePath))
            .Returns(fileStream);

        var result = await _fileService.GetFileAsync(fileEntity.Id);

        result.Should().NotBeNull();
        result!.FileName.Should().Be(fileName);
        result.ContentType.Should().Be(contentType);
        result.Stream.Should().NotBeNull();

        using var ms = new MemoryStream();
        await result.Stream!.CopyToAsync(ms);
        ms.ToArray().Should().BeEquivalentTo(content);

        await result.Stream.DisposeAsync();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}