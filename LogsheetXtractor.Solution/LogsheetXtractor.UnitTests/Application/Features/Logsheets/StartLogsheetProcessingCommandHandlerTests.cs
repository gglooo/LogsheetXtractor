using FluentAssertions;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Features.Logsheets;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Wolverine;
using Xunit;

namespace LogsheetXtractor.UnitTests.Application.Features.Logsheets;

public class StartLogsheetProcessingCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IMessageBus> _busMock = new();
    private readonly Mock<ICredentialCookieAccessor> _accessorMock = new();
    private readonly Mock<ILogger<StartLogsheetProcessingCommand>> _loggerMock = new();

    public StartLogsheetProcessingCommandHandlerTests() { }

    [Fact]
    public async Task Handle_ShouldSetProcessingStatusAndPublishEvent_WhenLogsheetIsValid()
    {
        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            Status = ELogSheetStatus.Pending,
            Template = null!,
            File = null!,
        };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        var options =
            new LogsheetXtractor.Application.Features.Logsheets.ProcessLogsheetDataOptions(
                UglyCheckboxes: true
            );
        var command = new StartLogsheetProcessingCommand(logsheet.Id, options);

        var result = await StartLogsheetProcessingHandler.Handle(
            command,
            _dbContext,
            _busMock.Object,
            _accessorMock.Object,
            _loggerMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();

        var updatedLogsheet = await _dbContext.Logsheets.FirstAsync(l => l.Id == logsheet.Id);
        updatedLogsheet.Status.Should().Be(ELogSheetStatus.Processing);

        _busMock.Verify(
            b =>
                b.PublishAsync(
                    It.Is<ProcessLogsheetDataCommand>(c =>
                        c.LogsheetId == logsheet.Id && c.Options == options
                    ),
                    It.IsAny<DeliveryOptions>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldPublishCredentialHandleHeader_WhenProtectedCookieIsValid()
    {
        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            Status = ELogSheetStatus.Pending,
            Template = null!,
            File = null!,
        };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        const string browserHandle = "0123456789abcdef0123456789abcdef";

        _accessorMock.Setup(a => a.GetCookie()).Returns(browserHandle);

        var result = await StartLogsheetProcessingHandler.Handle(
            new StartLogsheetProcessingCommand(logsheet.Id, null),
            _dbContext,
            _busMock.Object,
            _accessorMock.Object,
            _loggerMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        _busMock.Verify(
            b =>
                b.PublishAsync(
                    It.IsAny<ProcessLogsheetDataCommand>(),
                    It.Is<DeliveryOptions>(o =>
                        o.Headers.ContainsKey(CredentialsConstants.UserCredentialHandleHeaderName)
                        && o.Headers[CredentialsConstants.UserCredentialHandleHeaderName] == browserHandle
                        && !o.Headers.ContainsKey("UserCookie")
                        && !o.Headers.Values.Any(v => v != null && v.Contains("secret"))
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLogsheetNotFound()
    {
        var command = new StartLogsheetProcessingCommand(Guid.NewGuid(), null);

        var result = await StartLogsheetProcessingHandler.Handle(
            command,
            _dbContext,
            _busMock.Object,
            _accessorMock.Object,
            _loggerMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("not found"));

        _busMock.Verify(
            b =>
                b.PublishAsync(It.IsAny<ProcessLogsheetDataCommand>(), It.IsAny<DeliveryOptions>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLogsheetIsSoftDeleted()
    {
        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            Status = ELogSheetStatus.Pending,
            Template = null!,
            File = null!,
            DeletedAt = DateTime.UtcNow,
        };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        var result = await StartLogsheetProcessingHandler.Handle(
            new StartLogsheetProcessingCommand(logsheet.Id, null),
            _dbContext,
            _busMock.Object,
            _accessorMock.Object,
            _loggerMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("not found"));

        _busMock.Verify(
            b =>
                b.PublishAsync(It.IsAny<ProcessLogsheetDataCommand>(), It.IsAny<DeliveryOptions>()),
            Times.Never
        );
    }

    [Theory]
    [InlineData(ELogSheetStatus.Processing)]
    [InlineData(ELogSheetStatus.NeedsReview)]
    [InlineData(ELogSheetStatus.Completed)]
    public async Task Handle_ShouldFailWithoutPublishing_WhenLogsheetStateCannotBeProcessed(
        ELogSheetStatus status
    )
    {
        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            Status = status,
            Template = null!,
            File = null!,
        };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        var result = await StartLogsheetProcessingHandler.Handle(
            new StartLogsheetProcessingCommand(logsheet.Id, null),
            _dbContext,
            _busMock.Object,
            _accessorMock.Object,
            _loggerMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();

        var notUpdatedLogsheet = await _dbContext.Logsheets.FirstAsync(l => l.Id == logsheet.Id);
        notUpdatedLogsheet.Status.Should().Be(status);

        _busMock.Verify(
            b =>
                b.PublishAsync(It.IsAny<ProcessLogsheetDataCommand>(), It.IsAny<DeliveryOptions>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLogsheetNotInValidState()
    {
        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            Status = ELogSheetStatus.Completed,
            Template = null!,
            File = null!,
        };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        var command = new StartLogsheetProcessingCommand(logsheet.Id, null);

        var result = await StartLogsheetProcessingHandler.Handle(
            command,
            _dbContext,
            _busMock.Object,
            _accessorMock.Object,
            _loggerMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("valid state"));

        var notUpdatedLogsheet = await _dbContext.Logsheets.FirstAsync(l => l.Id == logsheet.Id);
        notUpdatedLogsheet.Status.Should().Be(ELogSheetStatus.Completed);

        _busMock.Verify(
            b =>
                b.PublishAsync(It.IsAny<ProcessLogsheetDataCommand>(), It.IsAny<DeliveryOptions>()),
            Times.Never
        );
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
