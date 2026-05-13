using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.ExtractedValues;
using LogsheetXtractor.Application.Features.File.DTOs;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.ValueObjects;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace LogsheetXtractor.UnitTests.Application.Features.ExtractedValues;

public class GetExtractedValueImageTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IExtractedValuesService> _extractedValuesServiceMock = new();

    [Fact]
    public async Task Handle_ShouldReturnImage_WhenExtractedValueExists()
    {
        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            Template = new LogsheetXtractor.Domain.Entities.Template
            {
                Name = "T",
                File = new LogsheetXtractor.Domain.Entities.File { StoredFileName = "t" },
                FrontsideTemplate = null,
            },
        };
        var roi = new Roi
        {
            Id = Guid.NewGuid(),
            Coordinates = new Coordinates(0, 0, 10, 10),
            Template = logsheet.Template,
        };
        var extractedValue = new ExtractedValue
        {
            Id = Guid.NewGuid(),
            LogsheetId = logsheet.Id,
            Logsheet = logsheet,
            RoiId = roi.Id,
            Roi = roi,
        };

        _dbContext.Logsheets.Add(logsheet);
        _dbContext.Rois.Add(roi);
        _dbContext.ExtractedValues.Add(extractedValue);
        await _dbContext.SaveChangesAsync();

        var query = new GetExtractedValueImageQuery(extractedValue.Id);
        var expectedDto = new GetFileDto
        {
            Stream = new MemoryStream(),
            ContentType = "image/png",
            FileName = "test.png",
        };

        _extractedValuesServiceMock
            .Setup(x =>
                x.GetExtractedValueImageAsync(
                    It.IsAny<GetExtractedValueImageDto>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok(expectedDto));

        var result = await GetExtractedValueImageHandler.Handle(
            query,
            _dbContext,
            _extractedValuesServiceMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenExtractedValueNotFound()
    {
        var query = new GetExtractedValueImageQuery(Guid.NewGuid());

        var result = await GetExtractedValueImageHandler.Handle(
            query,
            _dbContext,
            _extractedValuesServiceMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
    }

    [Fact]
    public async Task Handle_ShouldReturnServiceFailure_WhenLogsheetImageIsMissing()
    {
        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            FileId = Guid.NewGuid(),
            Template = new LogsheetXtractor.Domain.Entities.Template
            {
                Name = "T",
                File = new LogsheetXtractor.Domain.Entities.File { StoredFileName = "t" },
                FrontsideTemplate = null,
                Width = 100,
                Height = 200,
            },
        };
        var roi = new Roi
        {
            Id = Guid.NewGuid(),
            Coordinates = new Coordinates(0, 0, 10, 10),
            Template = logsheet.Template,
        };
        var extractedValue = new ExtractedValue
        {
            Id = Guid.NewGuid(),
            LogsheetId = logsheet.Id,
            Logsheet = logsheet,
            RoiId = roi.Id,
            Roi = roi,
        };

        _dbContext.Logsheets.Add(logsheet);
        _dbContext.Rois.Add(roi);
        _dbContext.ExtractedValues.Add(extractedValue);
        await _dbContext.SaveChangesAsync();

        _extractedValuesServiceMock
            .Setup(x =>
                x.GetExtractedValueImageAsync(
                    It.Is<GetExtractedValueImageDto>(dto =>
                        dto.ExtractedValueId == extractedValue.Id
                        && dto.LogsheetFileId == logsheet.FileId
                        && dto.PageNumber == 0
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Fail<GetFileDto>(new NotFoundError("Logsheet image not found")));

        var result = await GetExtractedValueImageHandler.Handle(
            new GetExtractedValueImageQuery(extractedValue.Id),
            _dbContext,
            _extractedValuesServiceMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
