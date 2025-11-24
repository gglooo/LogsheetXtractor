using FluentAssertions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.Logsheets;

public class ProcessLogsheetDataCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();

    [Fact]
    public async Task Handle_ShouldFail_WhenLogsheet_NotFound()
    {
        var command = new ProcessLogsheetDataCommand(Guid.NewGuid());

        var result = await ProcessLogsheetDataHandler.Handle(command, _dbContext);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Logsheet not found");
    }

    [Fact]
    public async Task Handle_ShouldThrowNotImplemented_WhenLogsheetExists()
    {
        var logsheetId = Guid.NewGuid();
        var logsheet = new Logsheet { Id = logsheetId, Template = null!, File = null! };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        var command = new ProcessLogsheetDataCommand(logsheetId);

        Func<Task> act = async () => await ProcessLogsheetDataHandler.Handle(command, _dbContext);

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
