using FluentAssertions;
using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.ExtractedValues.DTOs;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Application.Features.Logsheets;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;
using Microsoft.Extensions.Logging;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.Logsheets;

public class ResetLogsheetProofreadingTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ILogger<ResetLogsheetProofreadingCommand>> _loggerMock = new();

    [Fact]
    public async Task Handle_ShouldResetLogsheet_AndRemoveExtractedValues()
    {
        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            Status = ELogSheetStatus.NeedsReview,
            ProcessedAt = DateTime.UtcNow,
            Template = new Domain.Entities.Template
                { Name = "T", File = new Domain.Entities.File { StoredFileName = "t" } }
        };
        var extractedValue = new ExtractedValue
        {
            Id = Guid.NewGuid(),
            LogsheetId = logsheet.Id,
            Logsheet = logsheet
        };

        _dbContext.Logsheets.Add(logsheet);
        _dbContext.ExtractedValues.Add(extractedValue);
        await _dbContext.SaveChangesAsync();

        var command = new ResetLogsheetProofreadingCommand(logsheet.Id);

        var expectedDto = new LogsheetDetailDto
        (
            logsheet.Id,
            new TemplateListDto(logsheet.Template.Id, "T", null, null, null, 0, 0, 100, 100, DateTime.UtcNow),
            new FileDto(Guid.NewGuid(), "t", "t", 0, DateTime.UtcNow),
            ELogSheetStatus.Pending,
            null,
            null,
            new List<ExtractedValueDto>(),
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        _mapperMock.Setup(x =>
                x.Map<LogsheetDetailDto>(
                    It.Is<Logsheet>(l => l.Id == logsheet.Id && l.Status == ELogSheetStatus.Pending)))
            .Returns(expectedDto);

        var result =
            await ResetLogsheetProofreadingHandler.Handle(command, _dbContext, _loggerMock.Object,
                CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();

        var dbLogsheet = await _dbContext.Logsheets.FindAsync(logsheet.Id);
        dbLogsheet!.Status.Should().Be(ELogSheetStatus.Pending);
        dbLogsheet.ProcessedAt.Should().BeNull();
        dbLogsheet.CompletedAt.Should().BeNull();

        var dbValues = await _dbContext.ExtractedValues.Where(e => e.LogsheetId == logsheet.Id).ToListAsync();
        dbValues.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLogsheetNotFound()
    {
        var command = new ResetLogsheetProofreadingCommand(Guid.NewGuid());
        var result =
            await ResetLogsheetProofreadingHandler.Handle(command, _dbContext, _loggerMock.Object,
                CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}