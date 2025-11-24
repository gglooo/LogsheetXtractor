using FluentAssertions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Infrastructure.Services;
using Xunit;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Tests.Common;

namespace WebFormHTR.Tests.Infrastructure.Services;

public class FileServiceTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IMapper> _mapperMock;
    private readonly FileService _fileService;
    private readonly string _storageDirectory = "FileStorage";

    public FileServiceTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mapperMock = new Mock<IMapper>();
        _fileService = new FileService(_dbContext, _mapperMock.Object);

        if (Directory.Exists(_storageDirectory))
        {
            Directory.Delete(_storageDirectory, true);
        }
    }

    [Fact]
    public async Task UploadFileAsync_ShouldSaveFileToDiskAndDb()
    {
        var content = new byte[] { 1, 2, 3 };
        var fileName = "test.txt";
        var contentType = "text/plain";
        var expectedDto = new FileDto(Guid.NewGuid(), fileName, contentType, (uint)content.Length, DateTime.UtcNow);

        _mapperMock.Setup(x => x.Map<FileDto>(It.IsAny<WebFormHTR.Domain.Entities.File>()))
            .Returns(expectedDto);

        var result = await _fileService.UploadFileAsync(content, fileName, contentType);
        await _dbContext.SaveChangesAsync();

        result.Should().Be(expectedDto);

        var savedFile = await _dbContext.Files.FirstOrDefaultAsync();
        savedFile.Should().NotBeNull();
        savedFile!.OriginalFileName.Should().Be(fileName);

        File.Exists(savedFile.StoragePath).Should().BeTrue();
        var savedContent = await File.ReadAllBytesAsync(savedFile.StoragePath);
        savedContent.Should().BeEquivalentTo(content);
    }

    [Fact]
    public async Task GetFileAsync_ShouldReturnFile_WhenExists()
    {
        var content = new byte[] { 4, 5, 6 };
        var fileName = "existing.txt";
        var contentType = "text/plain";

        Directory.CreateDirectory(_storageDirectory);
        var storedFileName = $"{Guid.NewGuid()}_{fileName}";
        var storagePath = Path.Combine(_storageDirectory, storedFileName);
        await File.WriteAllBytesAsync(storagePath, content);

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
        _dbContext.Dispose();
    }
}