using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.ExtractedValues.DTOs;
using LogsheetXtractor.Application.Features.File.DTOs;
using LogsheetXtractor.Application.Features.Logsheets;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Features.Template.DTOs;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.Tests.Common;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace LogsheetXtractor.Tests.Application.Features.Logsheets;

public class PatchLogsheetCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IMapper> _mapperMock = new();

    [Fact]
    public async Task Handle_ShouldPatchLogsheet_WhenLogsheetExists()
    {
        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = Guid.NewGuid(),
            Name = "Template",
            File = new LogsheetXtractor.Domain.Entities.File { StoredFileName = "t.pdf" },
        };
        var file = new LogsheetXtractor.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            StoredFileName = "l.pdf",
        };

        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            Template = template,
            File = file,
            Status = LogsheetXtractor.Domain.Enums.ELogSheetStatus.Pending,
        };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        var patchDto = new PatchLogsheetDto("New Front Data", "New Back Data");
        var command = new PatchLogsheetCommand(logsheet.Id, patchDto);

        var expectedDto = new LogsheetDetailDto(
            logsheet.Id,
            new TemplateListDto(
                template.Id,
                template.Name,
                null,
                null,
                null,
                0,
                0,
                0,
                0,
                DateTime.UtcNow
            ),
            new FileDto(
                file.Id,
                file.OriginalFileName,
                file.ContentType,
                file.SizeBytes,
                file.CreatedAt
            ),
            logsheet.Status,
            null,
            null,
            new List<ExtractedValueDto>(),
            DateTime.UtcNow,
            null
        );

        _mapperMock.Setup(x => x.Map<LogsheetDetailDto>(It.IsAny<Logsheet>())).Returns(expectedDto);

        var result = await PatchLogsheetHandler.Handle(
            command,
            _dbContext,
            _mapperMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();

        result.IsSuccess.Should().BeTrue();

        var updatedLogsheet = await _dbContext.Logsheets.FindAsync(logsheet.Id);

        updatedLogsheet.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLogsheetNotFound()
    {
        var command = new PatchLogsheetCommand(Guid.NewGuid(), new PatchLogsheetDto(null, null));

        var result = await PatchLogsheetHandler.Handle(
            command,
            _dbContext,
            _mapperMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
        result.Errors.First().Message.Should().Be("Logsheet not found");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
