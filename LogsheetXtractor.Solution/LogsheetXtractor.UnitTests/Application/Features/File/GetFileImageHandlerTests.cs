using FluentAssertions;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.File;
using LogsheetXtractor.Application.Features.File.Interfaces;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using Moq;

namespace LogsheetXtractor.UnitTests.Application.Features.File;

public class GetFileImageHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IFileService> _fileServiceMock = new();

    [Fact]
    public async Task HandleAsync_ShouldReturnNotFound_WhenFileIsMissing()
    {
        var result = await GetFileImageHandler.HandleAsync(
            new GetFileImageQuery(Guid.NewGuid()),
            _dbContext,
            _fileServiceMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
        _fileServiceMock.Verify(
            x => x.ConvertToImageAsync(It.IsAny<Guid>()),
            Times.Never
        );
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnConstraintFailure_WhenConversionReturnsNull()
    {
        var file = new LogsheetXtractor.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            StoredFileName = "input.pdf",
        };
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        _fileServiceMock
            .Setup(x => x.ConvertToImageAsync(file.Id))
            .ReturnsAsync((GetFileDto?)null);

        var result = await GetFileImageHandler.HandleAsync(
            new GetFileImageQuery(file.Id),
            _dbContext,
            _fileServiceMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<ConstraintError>();
        result.Errors.Should().Contain(e => e.Message == "Failed to convert file to image");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnImageWithContentType_WhenConversionSucceeds()
    {
        var file = new LogsheetXtractor.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            StoredFileName = "input.pdf",
        };
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        var expectedFile = new GetFileDto
        {
            FileName = "input.png",
            ContentType = "image/png",
            Stream = new MemoryStream([1, 2, 3]),
        };
        _fileServiceMock
            .Setup(x => x.ConvertToImageAsync(file.Id))
            .ReturnsAsync(expectedFile);

        var result = await GetFileImageHandler.HandleAsync(
            new GetFileImageQuery(file.Id),
            _dbContext,
            _fileServiceMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(expectedFile);
        result.Value.ContentType.Should().Be("image/png");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
