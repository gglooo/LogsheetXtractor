using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.File.Interfaces;
using LogsheetXtractor.Application.Features.Logsheets;
using LogsheetXtractor.Application.Features.PdfCropper;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Domain.ValueObjects;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using Moq;

namespace LogsheetXtractor.UnitTests.Application.Features.Logsheets;

public class GetLogsheetImageTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IPdfCropperService> _pdfCropperServiceMock = new();
    private readonly Mock<IFileService> _fileServiceMock = new();

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenLogsheetIsMissing()
    {
        var result = await GetLogsheetImageHandler.Handle(
            new GetLogsheetImageQuery(Guid.NewGuid(), IsFrontside: true),
            _dbContext,
            _pdfCropperServiceMock.Object,
            _fileServiceMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenLogsheetFileCannotBeLoaded()
    {
        var logsheet = CreateLogsheet();
        await _dbContext.SaveChangesAsync();

        _fileServiceMock
            .Setup(x => x.GetFileAsync(logsheet.FileId))
            .ReturnsAsync((GetFileDto?)null);

        var result = await GetLogsheetImageHandler.Handle(
            new GetLogsheetImageQuery(logsheet.Id, IsFrontside: true),
            _dbContext,
            _pdfCropperServiceMock.Object,
            _fileServiceMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
        result.Errors.Should().Contain(e => e.Message == "Logsheet file not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnPng_WhenFrontsideImageIsWarped()
    {
        var logsheet = CreateLogsheet();
        await _dbContext.SaveChangesAsync();

        _fileServiceMock
            .Setup(x => x.GetFileAsync(logsheet.FileId))
            .ReturnsAsync(new GetFileDto { Stream = new MemoryStream([1, 2, 3]) });

        _pdfCropperServiceMock
            .Setup(x =>
                x.GetWarpedSection(
                    It.IsAny<byte[]>(),
                    0,
                    It.IsAny<IEnumerable<PointCoordinate>>(),
                    It.IsAny<IEnumerable<PointCoordinate>>(),
                    200,
                    400,
                    100,
                    200,
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Result.Ok<Stream>(new MemoryStream([4, 5, 6])));

        var result = await GetLogsheetImageHandler.Handle(
            new GetLogsheetImageQuery(logsheet.Id, IsFrontside: true),
            _dbContext,
            _pdfCropperServiceMock.Object,
            _fileServiceMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.ContentType.Should().Be("image/png");
        result.Value.FileName.Should().Be($"logsheet_{logsheet.Id}.png");
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenBacksidePageIsMissing()
    {
        var backsideTemplate = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = Guid.NewGuid(),
            Name = "Backside",
            Width = 100,
            Height = 200,
            File = new LogsheetXtractor.Domain.Entities.File { StoredFileName = "back.pdf" },
        };
        var logsheet = CreateLogsheet(backsideTemplate);
        await _dbContext.SaveChangesAsync();

        _fileServiceMock
            .Setup(x => x.GetFileAsync(logsheet.FileId))
            .ReturnsAsync(new GetFileDto { Stream = new MemoryStream([1, 2, 3]) });
        _pdfCropperServiceMock
            .Setup(x => x.GetPageCount(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .Returns(1);

        var result = await GetLogsheetImageHandler.Handle(
            new GetLogsheetImageQuery(logsheet.Id, IsFrontside: false),
            _dbContext,
            _pdfCropperServiceMock.Object,
            _fileServiceMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
        result.Errors.Should().Contain(e => e.Message == "Logsheet does not have a backside page");
    }

    private Logsheet CreateLogsheet(
        LogsheetXtractor.Domain.Entities.Template? backsideTemplate = null
    )
    {
        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = Guid.NewGuid(),
            Name = $"template-{Guid.NewGuid()}",
            Width = 100,
            Height = 200,
            File = new LogsheetXtractor.Domain.Entities.File { StoredFileName = "template.pdf" },
        };
        if (backsideTemplate is not null)
        {
            template.ForceSetBacksideTemplate(backsideTemplate);
        }
        var file = new LogsheetXtractor.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            StoredFileName = "logsheet.pdf",
        };
        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            Template = template,
            TemplateId = template.Id,
            File = file,
            FileId = file.Id,
            Status = ELogSheetStatus.Pending,
        };

        _dbContext.Logsheets.Add(logsheet);
        return logsheet;
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
