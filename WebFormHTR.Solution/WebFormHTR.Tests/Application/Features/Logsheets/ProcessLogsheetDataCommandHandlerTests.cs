using FluentAssertions;
using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Application.Interfaces;
using Wolverine;
using WebFormHTR.Application.Features.Logsheets.Events;
using WebFormHTR.Application.Extensions;
using WebFormHTR.Tests.Common;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.Logsheets;

public class ProcessLogsheetDataCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<ILogsheetService> _logsheetServiceMock = new();
    private readonly Mock<ICredentialCookieAccessor> _accessorMock = new();
    private readonly Mock<IMessageBus> _busMock = new();

    [Fact]
    public async Task Handle_ShouldFail_WhenLogsheet_NotFound()
    {
        var command = new ProcessLogsheetDataCommand(Guid.NewGuid());

        await ProcessLogsheetDataHandler.Handle(command, _dbContext, _busMock.Object, _logsheetServiceMock.Object,
            _accessorMock.Object,
            CancellationToken.None);

        _busMock.Verify(bus => bus.PublishAsync(
            It.Is<LogsheetProcessingFinishedEvent>(e =>
                e.LogsheetId == command.LogsheetId && e.IsSuccess == false && e.ErrorMessage == "Logsheet not found"),
            It.IsAny<DeliveryOptions>()), Times.Once);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}