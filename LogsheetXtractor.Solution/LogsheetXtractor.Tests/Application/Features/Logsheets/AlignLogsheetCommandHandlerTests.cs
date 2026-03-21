using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.ExtractedValues.DTOs;
using LogsheetXtractor.Application.Features.File.DTOs;
using LogsheetXtractor.Application.Features.Logsheets;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Features.Logsheets.Events;
using LogsheetXtractor.Application.Features.Template.DTOs;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.Tests.Common;
using Microsoft.Extensions.Logging;
using Moq;
using Wolverine;
using Xunit;

namespace LogsheetXtractor.Tests.Application.Features.Logsheets;

public class AlignLogsheetCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<ILogsheetService> _logsheetServiceMock = new();
    private readonly Mock<IMessageBus> _busMock = new();
    private readonly Mock<ILogger<AlignLogsheetCommand>> _loggerMock = new();

    [Fact]
    public async Task Handle_ShouldAlignLogsheet_WhenLogsheetExists()
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

        var command = new AlignLogsheetCommand(logsheet.Id);

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

        _logsheetServiceMock
            .Setup(x => x.AlignLogsheetAsync(It.IsAny<Logsheet>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await AlignLogsheetHandler.Handle(
            command,
            _dbContext,
            _logsheetServiceMock.Object,
            _busMock.Object,
            _loggerMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);
        _logsheetServiceMock.Verify(
            x =>
                x.AlignLogsheetAsync(
                    It.Is<Logsheet>(l => l.Id == logsheet.Id),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _busMock.Verify(
            b =>
                b.PublishAsync(
                    It.Is<LogsheetAutomaticallyAlignedEvent>(e => e.LogsheetId == logsheet.Id),
                    It.IsAny<DeliveryOptions>()
                ),
            Times.Once
        );
        _dbContext.ChangeTracker.HasChanges().Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLogsheetNotFound()
    {
        var command = new AlignLogsheetCommand(Guid.NewGuid());

        var result = await AlignLogsheetHandler.Handle(
            command,
            _dbContext,
            _logsheetServiceMock.Object,
            _busMock.Object,
            _loggerMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
        result.Errors.First().Message.Should().Be("Logsheet not found");
        _busMock.Verify(
            b =>
                b.PublishAsync(
                    It.IsAny<LogsheetAutomaticallyAlignedEvent>(),
                    It.IsAny<DeliveryOptions>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenAlignmentFails()
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

        var command = new AlignLogsheetCommand(logsheet.Id);
        var errorMessage = "Script engine failure";

        _logsheetServiceMock
            .Setup(x => x.AlignLogsheetAsync(It.IsAny<Logsheet>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Fail<LogsheetDetailDto>($"Failed to align logsheet: {errorMessage}")
            );

        var result = await AlignLogsheetHandler.Handle(
            command,
            _dbContext,
            _logsheetServiceMock.Object,
            _busMock.Object,
            _loggerMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be($"Failed to align logsheet: {errorMessage}");
        _busMock.Verify(
            b =>
                b.PublishAsync(
                    It.IsAny<LogsheetAutomaticallyAlignedEvent>(),
                    It.IsAny<DeliveryOptions>()
                ),
            Times.Never
        );
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
