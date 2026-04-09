using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.Features.Logsheets;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Features.Logsheets.Events;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Application.MessageProcessing;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wolverine;
using Xunit;

namespace LogsheetXtractor.UnitTests.Application.Features.Logsheets;

public class ProcessLogsheetDataCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<ILogsheetService> _logsheetServiceMock = new();
    private readonly Mock<ICredentialCookieAccessor> _accessorMock = new();
    private readonly Mock<IMessageBus> _busMock = new();

    [Fact]
    public async Task Handle_ShouldFail_WhenLogsheet_NotFound()
    {
        var command = new ProcessLogsheetDataCommand(Guid.NewGuid(), null);
        var envelope = CreateEnvelope(command, attempts: 1);

        await ProcessLogsheetDataHandler.Handle(
            command,
            _dbContext,
            envelope,
            _busMock.Object,
            _logsheetServiceMock.Object,
            _accessorMock.Object,
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
    public async Task Handle_ShouldRethrowRetryableException_WhenAttemptIsNotLast()
    {
        var logsheet = CreateProcessingLogsheet();
        var command = new ProcessLogsheetDataCommand(logsheet.Id, null);
        var retryPolicy = MessageRetryPolicies.For<ProcessLogsheetDataCommand>();
        var envelope = CreateEnvelope(command, retryPolicy.MaxAttempts - 1);

        _logsheetServiceMock
            .Setup(service =>
                service.ProcessLogsheetAsync(
                    It.IsAny<Logsheet>(),
                    command.Options,
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new TimeoutException("transient timeout"));

        var action = () =>
            ProcessLogsheetDataHandler.Handle(
                command,
                _dbContext,
                envelope,
                _busMock.Object,
                _logsheetServiceMock.Object,
                _accessorMock.Object,
                CancellationToken.None
            );

        await action.Should().ThrowAsync<TimeoutException>();

        var unchangedLogsheet = await _dbContext.Logsheets.FirstAsync(ls => ls.Id == logsheet.Id);
        unchangedLogsheet.Status.Should().Be(ELogSheetStatus.Processing);

        _busMock.Verify(
            bus =>
                bus.PublishAsync(
                    It.IsAny<LogsheetProcessingFinishedEvent>(),
                    It.IsAny<DeliveryOptions>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldFailLogsheet_WhenRetryableExceptionOccursOnLastAttempt()
    {
        var logsheet = CreateProcessingLogsheet();
        var command = new ProcessLogsheetDataCommand(logsheet.Id, null);
        var retryPolicy = MessageRetryPolicies.For<ProcessLogsheetDataCommand>();
        var envelope = CreateEnvelope(command, retryPolicy.MaxAttempts);

        _logsheetServiceMock
            .Setup(service =>
                service.ProcessLogsheetAsync(
                    It.IsAny<Logsheet>(),
                    command.Options,
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new TimeoutException("transient timeout"));

        await ProcessLogsheetDataHandler.Handle(
            command,
            _dbContext,
            envelope,
            _busMock.Object,
            _logsheetServiceMock.Object,
            _accessorMock.Object,
            CancellationToken.None
        );

        var failedLogsheet = await _dbContext.Logsheets.FirstAsync(ls => ls.Id == logsheet.Id);
        failedLogsheet.Status.Should().Be(ELogSheetStatus.Failed);
        failedLogsheet.ErrorMessage.Should().Contain("Exception during processing");
        failedLogsheet.ErrorMessage.Should().Contain("transient timeout");

        _busMock.Verify(
            bus =>
                bus.PublishAsync(
                    It.Is<LogsheetProcessingFinishedEvent>(evt =>
                        evt.LogsheetId == logsheet.Id
                        && !evt.IsSuccess
                        && evt.ErrorMessage != null
                        && evt.ErrorMessage.Contains("transient timeout")
                    ),
                    It.IsAny<DeliveryOptions>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldFailLogsheet_WhenExceptionIsNotRetryable()
    {
        var logsheet = CreateProcessingLogsheet();
        var command = new ProcessLogsheetDataCommand(logsheet.Id, null);
        var envelope = CreateEnvelope(command, attempts: 1);

        _logsheetServiceMock
            .Setup(service =>
                service.ProcessLogsheetAsync(
                    It.IsAny<Logsheet>(),
                    command.Options,
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new InvalidOperationException("unexpected failure"));

        await ProcessLogsheetDataHandler.Handle(
            command,
            _dbContext,
            envelope,
            _busMock.Object,
            _logsheetServiceMock.Object,
            _accessorMock.Object,
            CancellationToken.None
        );

        var failedLogsheet = await _dbContext.Logsheets.FirstAsync(ls => ls.Id == logsheet.Id);
        failedLogsheet.Status.Should().Be(ELogSheetStatus.Failed);
        failedLogsheet.ErrorMessage.Should().Contain("unexpected failure");

        _busMock.Verify(
            bus =>
                bus.PublishAsync(
                    It.Is<LogsheetProcessingFinishedEvent>(evt =>
                        evt.LogsheetId == logsheet.Id
                        && !evt.IsSuccess
                        && evt.ErrorMessage != null
                        && evt.ErrorMessage.Contains("unexpected failure")
                    ),
                    It.IsAny<DeliveryOptions>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldPublishFailure_WhenProcessingReturnsFailedResultWithoutThrowing()
    {
        var logsheet = CreateProcessingLogsheet();
        var command = new ProcessLogsheetDataCommand(logsheet.Id, null);
        var envelope = CreateEnvelope(command, attempts: 1);
        const string errorMessage = "Python HTR returned a failed result";

        _logsheetServiceMock
            .Setup(service =>
                service.ProcessLogsheetAsync(
                    It.IsAny<Logsheet>(),
                    command.Options,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Fail<LogsheetDetailDto>(errorMessage));

        await ProcessLogsheetDataHandler.Handle(
            command,
            _dbContext,
            envelope,
            _busMock.Object,
            _logsheetServiceMock.Object,
            _accessorMock.Object,
            CancellationToken.None
        );

        _busMock.Verify(
            bus =>
                bus.PublishAsync(
                    It.Is<LogsheetProcessingFinishedEvent>(evt =>
                        evt.LogsheetId == logsheet.Id
                        && !evt.IsSuccess
                        && evt.ErrorMessage != null
                        && evt.ErrorMessage.Contains(errorMessage)
                    ),
                    It.IsAny<DeliveryOptions>()
                ),
            Times.Once
        );
    }

    private Logsheet CreateProcessingLogsheet()
    {
        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            Status = ELogSheetStatus.Processing,
            Template = null!,
            File = null!,
        };

        _dbContext.Logsheets.Add(logsheet);
        _dbContext.SaveChanges();

        return logsheet;
    }

    private static Envelope CreateEnvelope(ProcessLogsheetDataCommand command, int attempts)
    {
        return new Envelope(command, Array.Empty<Envelope>()) { Attempts = attempts };
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
