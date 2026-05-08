using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Extensions;
using LogsheetXtractor.Application.Features.Logsheets;
using LogsheetXtractor.Application.Features.Logsheets.Events;
using LogsheetXtractor.Application.Features.Scripting;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using MapsterMapper;
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

        await ProcessLogsheetDataHandler.Handle(
            command,
            _dbContext,
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

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
