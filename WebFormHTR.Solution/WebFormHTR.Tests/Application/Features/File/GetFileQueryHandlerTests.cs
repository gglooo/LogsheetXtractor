using FluentAssertions;
using FluentResults;
using Moq;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Features.File;
using WebFormHTR.Application.Features.File.Interfaces;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.File;

public class GetFileQueryHandlerTests
{
    private readonly Mock<IFileService> _fileServiceMock;

    public GetFileQueryHandlerTests()
    {
        _fileServiceMock = new Mock<IFileService>();
    }

    [Fact]
    public async Task Handle_ShouldReturnFile_WhenFileExists()
    {
        var fileId = Guid.NewGuid();
        var expectedFile = new GetFileDto { Stream = new MemoryStream(), FileName = "test.txt", ContentType = "text/plain" };
        _fileServiceMock.Setup(x => x.GetFileAsync(fileId))
            .ReturnsAsync(expectedFile);

        var query = new GetFileQuery(fileId.ToString());

        var result = await GetFileHandler.Handle(query, _fileServiceMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedFile);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenServiceThrowsException()
    {
        var fileId = Guid.NewGuid();
        var errorMessage = "Service error";
        _fileServiceMock.Setup(x => x.GetFileAsync(fileId))
            .ThrowsAsync(new Exception(errorMessage));

        var query = new GetFileQuery(fileId.ToString());

        var result = await GetFileHandler.Handle(query, _fileServiceMock.Object);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == errorMessage);
    }
}