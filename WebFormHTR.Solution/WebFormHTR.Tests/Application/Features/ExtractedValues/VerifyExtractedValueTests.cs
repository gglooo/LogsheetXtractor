using FluentAssertions;
using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.ExtractedValues;
using WebFormHTR.Application.Features.ExtractedValues.DTOs;
using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Domain.ValueObjects;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.ExtractedValues;

public class VerifyExtractedValueTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IMapper> _mapperMock = new();

    [Fact]
    public async Task Handle_ShouldVerifyValue_WhenRequestIsValid()
    {
        var logsheet = new Logsheet { Id = Guid.NewGuid(), Status = ELogSheetStatus.NeedsReview };
        var roi = new Roi 
        { 
            Id = Guid.NewGuid(), 
            VariableName = "Var",
            Type = ERoiType.Handwritten,
            Coordinates = new Coordinates { X = 0, Y = 0, Width = 10, Height = 10 },
            Template = new Domain.Entities.Template { Name = "T", File = new Domain.Entities.File { StoredFileName = "t"} } 
        };
        var extractedValue = new ExtractedValue 
        { 
            Id = Guid.NewGuid(), 
            Value = "Initial", 
            LogsheetId = logsheet.Id,
            Logsheet = logsheet,
            RoiId = roi.Id,
            Roi = roi
        };

        _dbContext.Logsheets.Add(logsheet);
        _dbContext.Rois.Add(roi);
        _dbContext.ExtractedValues.Add(extractedValue);
        await _dbContext.SaveChangesAsync();

        var command = new VerifyExtractedValueCommand(extractedValue.Id, null);

        var expectedDto = new ExtractedValueDto
        (
            extractedValue.Id,
            logsheet.Id,
            roi.Id,
            ERoiType.Handwritten,
            "Var",
            "Initial",
            null,
            EVerificationStatus.Verified,
            DateTime.UtcNow,
            null
        );

        _mapperMock.Setup(x => x.Map<ExtractedValueDto>(It.Is<ExtractedValue>(e => e.Id == extractedValue.Id && e.Status == EVerificationStatus.Verified)))
            .Returns(expectedDto);

        var result = await VerifyExtractedValueHandler.Handle(command, _dbContext, _mapperMock.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);
        
        var dbValue = await _dbContext.ExtractedValues.FindAsync(extractedValue.Id);
        dbValue!.Status.Should().Be(EVerificationStatus.Verified);
        dbValue.CorrectedValue.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldVerifyAndCorrectValue_WhenCorrectionProvided()
    {
        var logsheet = new Logsheet { Id = Guid.NewGuid(), Status = ELogSheetStatus.NeedsReview };
        var roi = new Roi 
        { 
            Id = Guid.NewGuid(),
            VariableName = "Var",
            Type = ERoiType.Handwritten,
            Coordinates = new Coordinates { X = 0, Y = 0, Width = 10, Height = 10 },
            Template = new Domain.Entities.Template { Name = "T", File = new Domain.Entities.File { StoredFileName = "t"} } 
        };
        var extractedValue = new ExtractedValue 
        { 
            Id = Guid.NewGuid(), 
            Value = "Initial", 
            LogsheetId = logsheet.Id,
            Logsheet = logsheet,
            RoiId = roi.Id,
            Roi = roi
        };

        _dbContext.Logsheets.Add(logsheet);
        _dbContext.Rois.Add(roi);
        _dbContext.ExtractedValues.Add(extractedValue);
        await _dbContext.SaveChangesAsync();

        var command = new VerifyExtractedValueCommand(extractedValue.Id, "Corrected");

        var result = await VerifyExtractedValueHandler.Handle(command, _dbContext, _mapperMock.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var dbValue = await _dbContext.ExtractedValues.FindAsync(extractedValue.Id);
        dbValue!.Status.Should().Be(EVerificationStatus.Verified);
        dbValue.CorrectedValue.Should().Be("Corrected");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenExtractedValueNotFound()
    {
        var command = new VerifyExtractedValueCommand(Guid.NewGuid(), null);

        var result = await VerifyExtractedValueHandler.Handle(command, _dbContext, _mapperMock.Object, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLogsheetIsCompleted()
    {
        var logsheet = new Logsheet { Id = Guid.NewGuid(), Status = ELogSheetStatus.Completed };
        var roi = new Roi 
        { 
            Id = Guid.NewGuid(),
            Coordinates = new Coordinates { X = 0, Y = 0, Width = 10, Height = 10 },
            Template = new Domain.Entities.Template { Name = "T", File = new Domain.Entities.File { StoredFileName = "t"} } 
        };
        var extractedValue = new ExtractedValue 
        { 
            Id = Guid.NewGuid(), 
            Value = "Initial", 
            LogsheetId = logsheet.Id,
            Logsheet = logsheet,
            RoiId = roi.Id,
            Roi = roi
        };

        _dbContext.Logsheets.Add(logsheet);
        _dbContext.Rois.Add(roi);
        _dbContext.ExtractedValues.Add(extractedValue);
        await _dbContext.SaveChangesAsync();

        var command = new VerifyExtractedValueCommand(extractedValue.Id, null);

        var result = await VerifyExtractedValueHandler.Handle(command, _dbContext, _mapperMock.Object, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<InvalidStateError>();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
