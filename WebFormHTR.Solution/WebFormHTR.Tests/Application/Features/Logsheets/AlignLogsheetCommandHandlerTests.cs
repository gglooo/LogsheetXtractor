using FluentAssertions;
using FluentResults;
using Moq;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Features.ExtractedValues.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Features.File.DTOs;
using Microsoft.Extensions.Logging;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.Logsheets;

public class AlignLogsheetCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IHtrScriptEngine> _scriptEngineMock = new();
    private readonly Mock<ILogger<AlignLogsheetCommand>> _loggerMock = new();

    [Fact]
    public async Task Handle_ShouldAlignLogsheet_WhenLogsheetExists()
    {
        var template = new Domain.Entities.Template
            { Id = Guid.NewGuid(), Name = "Template", File = new Domain.Entities.File { StoredFileName = "t.pdf" } };
        var file = new Domain.Entities.File { Id = Guid.NewGuid(), StoredFileName = "l.pdf" };

        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            Template = template,
            File = file,
            Status = Domain.Enums.ELogSheetStatus.Pending
        };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        var command = new AlignLogsheetCommand(logsheet.Id);

        var expectedDto = new LogsheetDetailDto(logsheet.Id,
            new TemplateListDto(template.Id, template.Name, null, null, null, 0, 0, 0, 0, DateTime.UtcNow),
            new FileDto(file.Id, file.OriginalFileName, file.ContentType, file.SizeBytes, file.CreatedAt),
            logsheet.Status,
            null,
            null,
            new List<ExtractedValueDto>(),
            DateTime.UtcNow,
            null);

        _scriptEngineMock.Setup(x =>
                x.AutomaticAlignAsync(It.IsAny<AutomaticAlignmentInputDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result =
            await AlignLogsheetHandler.Handle(command, _dbContext, _scriptEngineMock.Object, _loggerMock.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);
        _scriptEngineMock.Verify(
            x => x.AutomaticAlignAsync(It.Is<AutomaticAlignmentInputDto>(i => i.Logsheet.Id == logsheet.Id),
                It.IsAny<CancellationToken>()), Times.Once);
        _dbContext.ChangeTracker.HasChanges().Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLogsheetNotFound()
    {
        var command = new AlignLogsheetCommand(Guid.NewGuid());

        var result =
            await AlignLogsheetHandler.Handle(command, _dbContext, _scriptEngineMock.Object, _loggerMock.Object, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
        result.Errors.First().Message.Should().Be("Logsheet not found");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenScriptEngineThrowsException()
    {
        var template = new Domain.Entities.Template
            { Id = Guid.NewGuid(), Name = "Template", File = new Domain.Entities.File { StoredFileName = "t.pdf" } };
        var file = new Domain.Entities.File { Id = Guid.NewGuid(), StoredFileName = "l.pdf" };

        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            Template = template,
            File = file,
            Status = Domain.Enums.ELogSheetStatus.Pending
        };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        var command = new AlignLogsheetCommand(logsheet.Id);
        var errorMessage = "Script engine failure";

        _scriptEngineMock.Setup(x =>
                x.AutomaticAlignAsync(It.IsAny<AutomaticAlignmentInputDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(errorMessage));

        var result =
            await AlignLogsheetHandler.Handle(command, _dbContext, _scriptEngineMock.Object, _loggerMock.Object, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be($"Failed to align logsheet: {errorMessage}");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}