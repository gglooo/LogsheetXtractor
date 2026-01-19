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
using WebFormHTR.Tests.Common;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.Logsheets;

public class ProcessLogsheetDataCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IHtrScriptEngine> _scriptEngineMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ILogsheetService> _logsheetServiceMock = new();

    [Fact]
    public async Task Handle_ShouldFail_WhenLogsheet_NotFound()
    {
        var command = new ProcessLogsheetDataCommand(Guid.NewGuid());

        var result =
            await ProcessLogsheetDataHandler.Handle(command, _dbContext, _logsheetServiceMock.Object, _scriptEngineMock.Object, _mapperMock.Object,
                CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Logsheet not found");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}