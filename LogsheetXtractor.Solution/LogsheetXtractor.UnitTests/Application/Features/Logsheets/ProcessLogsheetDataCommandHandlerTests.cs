using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.Features.Logsheets;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Features.Logsheets.Events;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wolverine;

namespace LogsheetXtractor.UnitTests.Application.Features.Logsheets;

public class ProcessLogsheetDataCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<ILogsheetService> _logsheetServiceMock = new();
    private readonly Mock<IMessageBus> _busMock = new();

    [Fact]
    public async Task Handle_ShouldFail_WhenLogsheet_NotFound()
    {
        var command = new ProcessLogsheetDataCommand(Guid.NewGuid(), null);

        await ProcessLogsheetDataHandler.Handle(
            command,
            _dbContext,
            _busMock.Object,
            _logsheetServiceMock.Object,
            CancellationToken.None
        );

        _busMock.Verify(
            bus =>
                bus.PublishAsync(
                    It.Is<LogsheetProcessingFinishedEvent>(e =>
                        e.LogsheetId == command.LogsheetId
                        && e.IsSuccess == false
                        && e.ErrorMessage == "Logsheet not found"
                    ),
                    It.IsAny<DeliveryOptions>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldUpdateStatusAndPublishSuccessEvent_WhenProcessingSucceeds()
    {
        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            Status = ELogSheetStatus.Processing,
            Template = null!,
            File = null!,
        };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        var options = new ProcessLogsheetDataOptions(UglyCheckboxes: true);
        _logsheetServiceMock
            .Setup(service =>
                service.ProcessLogsheetAsync(
                    It.Is<Logsheet>(l => l.Id == logsheet.Id),
                    options,
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback<Logsheet, ProcessLogsheetDataOptions?, CancellationToken>(
                (processedLogsheet, _, _) =>
                {
                    processedLogsheet.Status = ELogSheetStatus.NeedsReview;
                    processedLogsheet.ProcessedAt = DateTime.UtcNow;
                }
            )
            .ReturnsAsync(Result.Ok<LogsheetDetailDto>(null!));

        await ProcessLogsheetDataHandler.Handle(
            new ProcessLogsheetDataCommand(logsheet.Id, options),
            _dbContext,
            _busMock.Object,
            _logsheetServiceMock.Object,
            CancellationToken.None
        );

        var updatedLogsheet = await _dbContext.Logsheets.FirstAsync(l => l.Id == logsheet.Id);
        updatedLogsheet.Status.Should().Be(ELogSheetStatus.NeedsReview);
        updatedLogsheet.ProcessedAt.Should().NotBeNull();

        _busMock.Verify(
            bus =>
                bus.PublishAsync(
                    It.Is<LogsheetProcessingFinishedEvent>(e =>
                        e.LogsheetId == logsheet.Id
                        && e.IsSuccess
                        && e.ErrorMessage == null
                    ),
                    It.IsAny<DeliveryOptions>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldMarkFailedPreserveErrorAndPublishFailureEvent_WhenProcessingFails()
    {
        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            Status = ELogSheetStatus.Processing,
            Template = null!,
            File = null!,
        };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        _logsheetServiceMock
            .Setup(service =>
                service.ProcessLogsheetAsync(
                    It.Is<Logsheet>(l => l.Id == logsheet.Id),
                    null,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Fail<LogsheetDetailDto>("OCR failed: missing output"));

        await ProcessLogsheetDataHandler.Handle(
            new ProcessLogsheetDataCommand(logsheet.Id, null),
            _dbContext,
            _busMock.Object,
            _logsheetServiceMock.Object,
            CancellationToken.None
        );

        var updatedLogsheet = await _dbContext.Logsheets.FirstAsync(l => l.Id == logsheet.Id);
        updatedLogsheet.Status.Should().Be(ELogSheetStatus.Failed);
        updatedLogsheet.ErrorMessage.Should().Contain("OCR failed: missing output");

        _busMock.Verify(
            bus =>
                bus.PublishAsync(
                    It.Is<LogsheetProcessingFinishedEvent>(e =>
                        e.LogsheetId == logsheet.Id
                        && !e.IsSuccess
                        && e.ErrorMessage == "OCR failed: missing output"
                    ),
                    It.IsAny<DeliveryOptions>()
                ),
            Times.Once
        );
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
