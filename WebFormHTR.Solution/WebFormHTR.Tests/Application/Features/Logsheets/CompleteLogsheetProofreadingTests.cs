using FluentAssertions;
using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Application.Features.Logsheets;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;
using Microsoft.Extensions.Logging;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.Logsheets;

using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Features.ExtractedValues.DTOs;

public class CompleteLogsheetProofreadingTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ILogger<CompleteLogsheetProofreadingCommand>> _loggerMock = new();

    [Fact]
    public async Task Handle_ShouldCompleteLogsheet_WhenAllValuesVerified()
    {
        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            Status = ELogSheetStatus.NeedsReview,
            Template = new Domain.Entities.Template
                { Name = "T", File = new Domain.Entities.File { StoredFileName = "t" } }
        };
        var extractedValue = new ExtractedValue
        {
            Id = Guid.NewGuid(),
            Status = EVerificationStatus.Verified,
            LogsheetId = logsheet.Id,
            Logsheet = logsheet
        };

        _dbContext.Logsheets.Add(logsheet);


        _dbContext.ExtractedValues.Add(extractedValue);
        await _dbContext.SaveChangesAsync();

        var command = new CompleteLogsheetProofreadingCommand(logsheet.Id);
        var expectedDto = new LogsheetDetailDto
        (
            logsheet.Id,
            new TemplateListDto(logsheet.Template.Id, "T", null, null, null, 0, 0, 100, 100, DateTime.UtcNow),
            new FileDto(Guid.NewGuid(), "t", "t", 0, DateTime.UtcNow),
            ELogSheetStatus.Completed,
            null,
            null,
            new List<ExtractedValueDto>(),
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        _mapperMock.Setup(x =>
                x.Map<LogsheetDetailDto>(It.Is<Logsheet>(l =>
                    l.Id == logsheet.Id && l.Status == ELogSheetStatus.Completed)))
            .Returns(expectedDto);

        var result =
            await CompleteLogsheetProofreadingHandler.Handle(command, _dbContext, _mapperMock.Object, _loggerMock.Object,
                CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);
        result.Value.Status.Should().Be(ELogSheetStatus.Completed);

        var dbLogsheet = await _dbContext.Logsheets.FindAsync(logsheet.Id);
        dbLogsheet!.Status.Should().Be(ELogSheetStatus.Completed);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLogsheetNotFound()
    {
        var command = new CompleteLogsheetProofreadingCommand(Guid.NewGuid());
        var result =
            await CompleteLogsheetProofreadingHandler.Handle(command, _dbContext, _mapperMock.Object, _loggerMock.Object,
                CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenValuesNotVerified()
    {
        var logsheet = new Logsheet { Id = Guid.NewGuid(), Status = ELogSheetStatus.NeedsReview };
        var extractedValue = new ExtractedValue
        {
            Id = Guid.NewGuid(),
            Status = EVerificationStatus.Unverified,
            LogsheetId = logsheet.Id,
            Logsheet = logsheet
        };

        _dbContext.Logsheets.Add(logsheet);
        _dbContext.ExtractedValues.Add(extractedValue);
        await _dbContext.SaveChangesAsync();

        var command = new CompleteLogsheetProofreadingCommand(logsheet.Id);
        var result =
            await CompleteLogsheetProofreadingHandler.Handle(command, _dbContext, _mapperMock.Object, _loggerMock.Object,
                CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<InvalidStateError>();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenStatusNotNeedsReview()
    {
        var logsheet = new Logsheet { Id = Guid.NewGuid(), Status = ELogSheetStatus.Pending };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        var command = new CompleteLogsheetProofreadingCommand(logsheet.Id);
        var result =
            await CompleteLogsheetProofreadingHandler.Handle(command, _dbContext, _mapperMock.Object, _loggerMock.Object,
                CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<InvalidStateError>();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}