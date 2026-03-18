using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WebFormHTR.Application.Features.Logsheets;
using WebFormHTR.Application.Features.Logsheets.Create;
using WebFormHTR.Application.Features.Logsheets.Create.Events;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;
using Wolverine;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.Logsheets;

public class HandleLogsheetCreatedHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<ILogger<LogsheetCreatedEvent>> _loggerMock = new();
    private readonly Mock<IMessageBus> _busMock = new();

    [Fact]
    public async Task Handle_ShouldSetStatusToAligning_AndPublishAlignCommand_WhenPending()
    {
        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            Template = new Domain.Entities.Template { File = new Domain.Entities.File { StoredFileName = "t.pdf" } },
            File = new Domain.Entities.File { StoredFileName = "f.pdf" },
            Status = ELogSheetStatus.Pending
        };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        await HandleLogsheetCreatedHandler.Handle(
            new LogsheetCreatedEvent(logsheet.Id),
            _dbContext,
            _loggerMock.Object,
            _busMock.Object,
            CancellationToken.None);


        var saved = await _dbContext.Logsheets.FindAsync(logsheet.Id);
        saved!.Status.Should().Be(ELogSheetStatus.Aligning);
        _busMock.Verify(
            b => b.PublishAsync(
                It.Is<AlignLogsheetCommand>(c => c.LogsheetId == logsheet.Id),
                It.IsAny<DeliveryOptions>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSkip_WhenAutomaticAlignmentIsDisabled()
    {
        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            Template = new Domain.Entities.Template { File = new Domain.Entities.File { StoredFileName = "t.pdf" } },
            File = new Domain.Entities.File { StoredFileName = "f.pdf" },
            Status = ELogSheetStatus.Pending
        };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        await HandleLogsheetCreatedHandler.Handle(
            new LogsheetCreatedEvent(logsheet.Id, false),
            _dbContext,
            _loggerMock.Object,
            _busMock.Object,
            CancellationToken.None);

        var saved = await _dbContext.Logsheets.FindAsync(logsheet.Id);
        saved!.Status.Should().Be(ELogSheetStatus.Pending);
        _busMock.Verify(
            b => b.PublishAsync(It.IsAny<AlignLogsheetCommand>(), It.IsAny<DeliveryOptions>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSkip_WhenStatusIsNotPending()
    {
        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            Template = new Domain.Entities.Template { File = new Domain.Entities.File { StoredFileName = "t.pdf" } },
            File = new Domain.Entities.File { StoredFileName = "f.pdf" },
            Status = ELogSheetStatus.Completed
        };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        await HandleLogsheetCreatedHandler.Handle(
            new LogsheetCreatedEvent(logsheet.Id),
            _dbContext,
            _loggerMock.Object,
            _busMock.Object,
            CancellationToken.None);

        var saved = await _dbContext.Logsheets.FindAsync(logsheet.Id);
        saved!.Status.Should().Be(ELogSheetStatus.Completed);
        _busMock.Verify(
            b => b.PublishAsync(It.IsAny<AlignLogsheetCommand>(), It.IsAny<DeliveryOptions>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSkip_WhenLogsheetNotFound()
    {
        await HandleLogsheetCreatedHandler.Handle(
            new LogsheetCreatedEvent(Guid.NewGuid()),
            _dbContext,
            _loggerMock.Object,
            _busMock.Object,
            CancellationToken.None);

        _busMock.Verify(
            b => b.PublishAsync(It.IsAny<AlignLogsheetCommand>(), It.IsAny<DeliveryOptions>()),
            Times.Never);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
