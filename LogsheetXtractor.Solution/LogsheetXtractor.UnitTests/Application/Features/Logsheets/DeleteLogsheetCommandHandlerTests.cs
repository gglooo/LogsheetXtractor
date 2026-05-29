using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.File.Interfaces;
using LogsheetXtractor.Application.Features.Logsheets;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace LogsheetXtractor.UnitTests.Application.Features.Logsheets;

public class DeleteLogsheetCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IFileService> _fileServiceMock = new();

    public DeleteLogsheetCommandHandlerTests()
    {
        _dbContext = TestDbContextFactory.Create();
    }

    [Fact]
    public async Task Handle_ShouldDeleteLogsheet_WhenExists()
    {
        var logsheetId = Guid.NewGuid();
        var logsheet = new Logsheet
        {
            Id = logsheetId,
            Template = null!,
            File = null!,
        };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteLogsheetCommand(logsheetId);

        var result = await DeleteLogsheetHandler.Handle(
            command,
            _fileServiceMock.Object,
            _dbContext,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        var deletedLogsheet = await _dbContext.Logsheets.FindAsync(logsheetId);

        deletedLogsheet.Should().NotBeNull();
        deletedLogsheet.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLogsheet_NotFound()
    {
        var command = new DeleteLogsheetCommand(Guid.NewGuid());

        var result = await DeleteLogsheetHandler.Handle(
            command,
            _fileServiceMock.Object,
            _dbContext,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Logsheet not found");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
