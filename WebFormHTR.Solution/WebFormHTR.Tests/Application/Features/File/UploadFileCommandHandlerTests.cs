using FluentAssertions;
using FluentResults;
using Moq;
using WebFormHTR.Application.Features.File;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Application.Features.File.Interfaces;
using WebFormHTR.Application.Interfaces;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.File;

public class UploadFileCommandHandlerTests
{
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<IAppDbContext> _dbContextMock;

    public UploadFileCommandHandlerTests()
    {
        _fileServiceMock = new Mock<IFileService>();
        _dbContextMock = new Mock<IAppDbContext>();
    }

    [Fact]
    public async Task Handle_ShouldUploadFileAndSaveChanges_WhenSuccessful()
    {
        var command = new UploadFileCommand([1, 2, 3], "test.txt", "text/plain");
        var expectedFileDto = new FileDto(Guid.NewGuid(), "test.txt", "text/plain", 3, DateTime.UtcNow);

        _fileServiceMock.Setup(x => x.UploadFileAsync(command.FileContent, command.FileName, command.ContentType))
            .ReturnsAsync(expectedFileDto);

        var result = await UploadFileHandler.Handle(command, _fileServiceMock.Object, _dbContextMock.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedFileDto);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenServiceThrowsException()
    {
        var command = new UploadFileCommand([1, 2, 3], "test.txt", "text/plain");
        var errorMessage = "Upload failed";

        _fileServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception(errorMessage));

        var result = await UploadFileHandler.Handle(command, _fileServiceMock.Object, _dbContextMock.Object, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == errorMessage);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}