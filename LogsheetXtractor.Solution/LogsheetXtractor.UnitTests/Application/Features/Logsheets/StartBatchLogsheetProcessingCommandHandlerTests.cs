using FluentResults;
using FluentAssertions;
using LogsheetXtractor.Application.Errors;
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

public class StartBatchLogsheetProcessingCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IMessageBus> _busMock = new();
    private readonly Mock<ICredentialCookieAccessor> _accessorMock = new();
    private readonly Mock<IUserCredentialCookieProtector> _cookieProtectorMock = new();
    private readonly Mock<IUserCredentialHandleStore> _credentialHandleStoreMock = new();
    private readonly Mock<ILogger<StartBatchLogsheetProcessingCommand>> _loggerMock = new();

    public StartBatchLogsheetProcessingCommandHandlerTests() { }

    [Fact]
    public async Task Handle_ShouldSetProcessingStatusAndPublishEvent_WhenLogsheetsAreValid()
    {
        var logsheet1 = new Logsheet
        {
            Id = Guid.NewGuid(),
            Status = ELogSheetStatus.Pending,
            Template = null!,
            File = null!,
        };
        var logsheet2 = new Logsheet
        {
            Id = Guid.NewGuid(),
            Status = ELogSheetStatus.Failed,
            Template = null!,
            File = null!,
        };
        var logsheet3 = new Logsheet
        {
            Id = Guid.NewGuid(),
            Status = ELogSheetStatus.Completed,
            Template = null!,
            File = null!,
        }; // Invalid state

        _dbContext.Logsheets.AddRange(logsheet1, logsheet2, logsheet3);
        await _dbContext.SaveChangesAsync();

        var options =
            new LogsheetXtractor.Application.Features.Logsheets.ProcessLogsheetDataOptions(
                UglyCheckboxes: true
            );
        var command = new StartBatchLogsheetProcessingCommand(
            new[] { logsheet1.Id, logsheet2.Id, logsheet3.Id },
            options
        );

        var result = await StartBatchLogsheetProcessingHandler.Handle(
            command,
            _dbContext,
            _busMock.Object,
            _accessorMock.Object,
            _cookieProtectorMock.Object,
            _credentialHandleStoreMock.Object,
            _loggerMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();

        var updatedLogsheet1 = await _dbContext.Logsheets.FirstAsync(l => l.Id == logsheet1.Id);
        updatedLogsheet1.Status.Should().Be(ELogSheetStatus.Processing);

        var updatedLogsheet2 = await _dbContext.Logsheets.FirstAsync(l => l.Id == logsheet2.Id);
        updatedLogsheet2.Status.Should().Be(ELogSheetStatus.Processing);

        var notUpdatedLogsheet3 = await _dbContext.Logsheets.FirstAsync(l => l.Id == logsheet3.Id);
        notUpdatedLogsheet3.Status.Should().Be(ELogSheetStatus.Completed);

        _busMock.Verify(
            b =>
                b.PublishAsync(
                    It.Is<ProcessLogsheetDataCommand>(c =>
                        c.LogsheetId == logsheet1.Id && c.Options == options
                    ),
                    It.IsAny<DeliveryOptions>()
                ),
            Times.Once
        );

        _busMock.Verify(
            b =>
                b.PublishAsync(
                    It.Is<ProcessLogsheetDataCommand>(c =>
                        c.LogsheetId == logsheet2.Id && c.Options == options
                    ),
                    It.IsAny<DeliveryOptions>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldPublishCredentialHandleHeaders_WhenProtectedCookieIsValid()
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

        const string cookie = "v1:protected-cookie";
        const string handle = "credential-handle";
        var credentials = new Dictionary<ECredentialType, string>
        {
            [ECredentialType.Azure] = "raw-secret",
        };

        _accessorMock.Setup(a => a.GetCookie()).Returns(cookie);
        _cookieProtectorMock.Setup(p => p.Unprotect(cookie)).Returns(credentials);
        _credentialHandleStoreMock
            .Setup(s => s.CreateAsync(credentials, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(handle));

        var result = await StartBatchLogsheetProcessingHandler.Handle(
            new StartBatchLogsheetProcessingCommand(new[] { logsheet.Id }, null),
            _dbContext,
            _busMock.Object,
            _accessorMock.Object,
            _cookieProtectorMock.Object,
            _credentialHandleStoreMock.Object,
            _loggerMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        _busMock.Verify(
            b =>
                b.PublishAsync(
                    It.IsAny<ProcessLogsheetDataCommand>(),
                    It.Is<DeliveryOptions>(o =>
                        o.Headers.ContainsKey(CredentialsConstants.BackgroundHandleHeaderName)
                        && o.Headers[CredentialsConstants.BackgroundHandleHeaderName] == handle
                        && !o.Headers.ContainsKey("UserCookie")
                        && !o.Headers.Values.Any(v => v != null && v.Contains("raw-secret"))
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenNoValidLogsheetsFound()
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

        var command = new StartBatchLogsheetProcessingCommand(new[] { logsheet.Id }, null);

        var result = await StartBatchLogsheetProcessingHandler.Handle(
            command,
            _dbContext,
            _busMock.Object,
            _accessorMock.Object,
            _cookieProtectorMock.Object,
            _credentialHandleStoreMock.Object,
            _loggerMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("No valid logsheets"));

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
