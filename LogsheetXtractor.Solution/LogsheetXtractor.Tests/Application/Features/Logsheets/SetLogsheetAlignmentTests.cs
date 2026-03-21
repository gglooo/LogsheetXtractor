using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.ExtractedValues.DTOs;
using LogsheetXtractor.Application.Features.File.DTOs;
using LogsheetXtractor.Application.Features.Logsheets;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Features.Template.DTOs;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Domain.ValueObjects;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.Tests.Common;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LogsheetXtractor.Tests.Application.Features.Logsheets;

public class SetLogsheetAlignmentTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ILogger<SetLogsheetAlignmentCommand>> _loggerMock = new();

    [Fact]
    public async Task Handle_ShouldUpdateAlignment_WhenLogsheetExists()
    {
        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            Template = new LogsheetXtractor.Domain.Entities.Template
            {
                Name = "T",
                File = new LogsheetXtractor.Domain.Entities.File { StoredFileName = "t" },
            },
        };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        var alignmentDataDto = new AlignmentDataDto(
            new List<PointCoordinateDto>(),
            new List<PointCoordinateDto>
            {
                new() { X = 1, Y = 1 },
            }
        );
        var alignmentContainer = new AlignmentContainer(null, null);

        var command = new SetLogsheetAlignmentCommand(logsheet.Id, alignmentDataDto);

        var expectedDto = new LogsheetDetailDto(
            logsheet.Id,
            new TemplateListDto(
                logsheet.Template.Id,
                "T",
                null,
                null,
                null,
                0,
                0,
                100,
                100,
                DateTime.UtcNow
            ),
            new FileDto(Guid.NewGuid(), "t", "t", 0, DateTime.UtcNow),
            ELogSheetStatus.Pending,
            null,
            alignmentDataDto,
            new List<ExtractedValueDto>(),
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        _mapperMock
            .Setup(x => x.Map<AlignmentContainer>(alignmentDataDto))
            .Returns(alignmentContainer);

        _mapperMock
            .Setup(x => x.Map<LogsheetDetailDto>(It.Is<Logsheet>(l => l.Id == logsheet.Id)))
            .Returns(expectedDto);

        var result = await SetLogsheetAlignmentHandler.Handle(
            command,
            _dbContext,
            _mapperMock.Object,
            _loggerMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);

        var dbLogsheet = await _dbContext.Logsheets.FindAsync(logsheet.Id);
        dbLogsheet!.AlignmentData.Should().BeEquivalentTo(alignmentContainer);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLogsheetNotFound()
    {
        var command = new SetLogsheetAlignmentCommand(
            Guid.NewGuid(),
            new AlignmentDataDto(null, null)
        );

        var result = await SetLogsheetAlignmentHandler.Handle(
            command,
            _dbContext,
            _mapperMock.Object,
            _loggerMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
