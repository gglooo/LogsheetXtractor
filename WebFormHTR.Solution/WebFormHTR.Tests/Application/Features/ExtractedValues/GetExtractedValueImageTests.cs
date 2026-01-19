using FluentAssertions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.ExtractedValues;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.ValueObjects;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.ExtractedValues;

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
            Template = new Domain.Entities.Template { Name = "T", File = new Domain.Entities.File { StoredFileName = "t"} } 
        };
        var roi = new Roi 
        { 
            Id = Guid.NewGuid(),
            Coordinates = new Coordinates { X = 0, Y = 0, Width = 10, Height = 10 }
        };
        var extractedValue = new ExtractedValue 
        { 
            Id = Guid.NewGuid(), 
            LogsheetId = logsheet.Id,
            Logsheet = logsheet,
            RoiId = roi.Id,
            Roi = roi
        };

        _dbContext.Logsheets.Add(logsheet);
        _dbContext.Rois.Add(roi);
        _dbContext.ExtractedValues.Add(extractedValue);
        await _dbContext.SaveChangesAsync();

        var query = new GetExtractedValueImageQuery(extractedValue.Id);
        var expectedDto = new GetFileDto { Stream = new MemoryStream(), ContentType = "image/png", FileName = "test.png" };

        _extractedValuesServiceMock.Setup(x => x.GetExtractedValueImageAsync(It.Is<ExtractedValue>(e => e.Id == extractedValue.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(expectedDto));

        var result = await GetExtractedValueImageHandler.Handle(query, _dbContext, _extractedValuesServiceMock.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenExtractedValueNotFound()
    {
        var query = new GetExtractedValueImageQuery(Guid.NewGuid());

        var result = await GetExtractedValueImageHandler.Handle(query, _dbContext, _extractedValuesServiceMock.Object, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
