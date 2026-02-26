using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;
using Wolverine;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.Logsheets;

public class StartLogsheetProcessingCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IMessageBus> _busMock = new();
    private readonly Mock<ICredentialCookieAccessor> _accessorMock = new();
    private readonly Mock<ILogger<StartLogsheetProcessingCommand>> _loggerMock = new();

    public StartLogsheetProcessingCommandHandlerTests()
    {
    }

    [Fact]
    public async Task Handle_ShouldSetProcessingStatusAndPublishEvent_WhenLogsheetIsValid()
    {
        var logsheet = new Logsheet { Id = Guid.NewGuid(), Status = ELogSheetStatus.Pending, Template = null!, File = null! };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        var options = new WebFormHTR.Application.Features.Logsheets.ProcessLogsheetDataOptions(UglyCheckboxes: true);
        var command = new StartLogsheetProcessingCommand(logsheet.Id, options);

        var result = await StartLogsheetProcessingHandler.Handle(command, _dbContext, _busMock.Object, _accessorMock.Object, _loggerMock.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        
        var updatedLogsheet = await _dbContext.Logsheets.FirstAsync(l => l.Id == logsheet.Id);
        updatedLogsheet.Status.Should().Be(ELogSheetStatus.Processing);

        _busMock.Verify(b => b.PublishAsync(It.Is<ProcessLogsheetDataCommand>(c => c.LogsheetId == logsheet.Id && c.Options == options), It.IsAny<DeliveryOptions>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLogsheetNotFound()
    {
        var command = new StartLogsheetProcessingCommand(Guid.NewGuid(), null);

        var result = await StartLogsheetProcessingHandler.Handle(command, _dbContext, _busMock.Object, _accessorMock.Object, _loggerMock.Object, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("not found"));
        
        _busMock.Verify(b => b.PublishAsync(It.IsAny<ProcessLogsheetDataCommand>(), It.IsAny<DeliveryOptions>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLogsheetNotInValidState()
    {
        var logsheet = new Logsheet { Id = Guid.NewGuid(), Status = ELogSheetStatus.Completed, Template = null!, File = null! };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        var command = new StartLogsheetProcessingCommand(logsheet.Id, null);

        var result = await StartLogsheetProcessingHandler.Handle(command, _dbContext, _busMock.Object, _accessorMock.Object, _loggerMock.Object, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("valid state"));

        var notUpdatedLogsheet = await _dbContext.Logsheets.FirstAsync(l => l.Id == logsheet.Id);
        notUpdatedLogsheet.Status.Should().Be(ELogSheetStatus.Completed);

        _busMock.Verify(b => b.PublishAsync(It.IsAny<ProcessLogsheetDataCommand>(), It.IsAny<DeliveryOptions>()), Times.Never);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
