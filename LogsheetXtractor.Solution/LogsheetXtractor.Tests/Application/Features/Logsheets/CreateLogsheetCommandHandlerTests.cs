using FluentAssertions;
using LogsheetXtractor.Application.Features.ExtractedValues.DTOs;
using LogsheetXtractor.Application.Features.File.DTOs;
using LogsheetXtractor.Application.Features.Logsheets;
using LogsheetXtractor.Application.Features.Logsheets.Create;
using LogsheetXtractor.Application.Features.Logsheets.Create.Events;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Features.Template.DTOs;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.Tests.Common;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wolverine;
using Xunit;

namespace LogsheetXtractor.Tests.Application.Features.Logsheets;

public class CreateLogsheetCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMessageBus> _busMock;

    public CreateLogsheetCommandHandlerTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mapperMock = new Mock<IMapper>();
        _busMock = new Mock<IMessageBus>();
    }

    [Fact]
    public async Task Handle_ShouldCreateAndPublishEvent_WhenCreatedLogsheetIsPending()
    {
        var templateId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        var templateFile = new LogsheetXtractor.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "template.pdf",
            StoredFileName = "template.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };
        _dbContext.Files.Add(templateFile);

        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = templateId,
            Name = "Template",
            FileId = templateFile.Id,
        };
        var file = new LogsheetXtractor.Domain.Entities.File
        {
            Id = fileId,
            OriginalFileName = "logsheet.jpg",
            StoragePath = "path",
        };
        _dbContext.Templates.Add(template);
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        var command = new CreateLogsheetCommand(templateId, null, fileId);
        var mappedLogsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            FileId = fileId,
            Template = template,
            File = file,
            Status = ELogSheetStatus.Pending,
        };

        _mapperMock.Setup(m => m.Map<Logsheet>(command)).Returns(mappedLogsheet);

        var expectedDto = new LogsheetDetailDto(
            mappedLogsheet.Id,
            new TemplateListDto(
                templateId,
                "Template",
                null,
                null,
                null,
                0,
                0,
                0,
                0,
                DateTime.UtcNow
            ),
            new FileDto(fileId, "logsheet.jpg", "image/jpeg", 100, DateTime.UtcNow),
            ELogSheetStatus.Pending,
            DateTime.UtcNow,
            null,
            new List<ExtractedValueDto>(),
            DateTime.UtcNow,
            null
        );

        _mapperMock
            .Setup(m => m.Map<LogsheetDetailDto>(It.Is<Logsheet>(l => l.Id == mappedLogsheet.Id)))
            .Returns(expectedDto);

        var result = await CreateLogsheetHandler.Handle(
            command,
            CancellationToken.None,
            _dbContext,
            _mapperMock.Object,
            _busMock.Object
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);

        _busMock.Verify(
            b =>
                b.PublishAsync(
                    It.Is<LogsheetCreatedEvent>(e =>
                        e.LogsheetId == mappedLogsheet.Id && e.PerformAutomaticAlignment
                    ),
                    It.IsAny<DeliveryOptions>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateAndPublishEvent_WhenCreatedLogsheetIsNotPending()
    {
        var templateId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        var templateFile = new LogsheetXtractor.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "template.pdf",
            StoredFileName = "template.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };
        _dbContext.Files.Add(templateFile);

        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = templateId,
            Name = "Template",
            FileId = templateFile.Id,
        };
        var file = new LogsheetXtractor.Domain.Entities.File
        {
            Id = fileId,
            OriginalFileName = "logsheet.jpg",
            StoragePath = "path",
        };
        _dbContext.Templates.Add(template);
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        var command = new CreateLogsheetCommand(templateId, null, fileId);
        var mappedLogsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            FileId = fileId,
            Template = template,
            File = file,
            Status = ELogSheetStatus.Completed,
        };

        _mapperMock.Setup(m => m.Map<Logsheet>(command)).Returns(mappedLogsheet);
        _mapperMock
            .Setup(m => m.Map<LogsheetDetailDto>(It.IsAny<Logsheet>()))
            .Returns(
                new LogsheetDetailDto(
                    mappedLogsheet.Id,
                    new TemplateListDto(
                        templateId,
                        "Template",
                        null,
                        null,
                        null,
                        0,
                        0,
                        0,
                        0,
                        DateTime.UtcNow
                    ),
                    new FileDto(fileId, "logsheet.jpg", "image/jpeg", 100, DateTime.UtcNow),
                    ELogSheetStatus.Completed,
                    null,
                    null,
                    new List<ExtractedValueDto>(),
                    DateTime.UtcNow,
                    null
                )
            );

        var result = await CreateLogsheetHandler.Handle(
            command,
            CancellationToken.None,
            _dbContext,
            _mapperMock.Object,
            _busMock.Object
        );

        result.IsSuccess.Should().BeTrue();

        _busMock.Verify(
            b =>
                b.PublishAsync(
                    It.Is<LogsheetCreatedEvent>(e =>
                        e.LogsheetId == mappedLogsheet.Id && e.PerformAutomaticAlignment
                    ),
                    It.IsAny<DeliveryOptions>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenFileNotFound()
    {
        var command = new CreateLogsheetCommand(Guid.NewGuid(), null, Guid.NewGuid());

        var result = await CreateLogsheetHandler.Handle(
            command,
            CancellationToken.None,
            _dbContext,
            _mapperMock.Object,
            _busMock.Object
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "File not found");
        _busMock.Verify(
            b => b.PublishAsync(It.IsAny<LogsheetCreatedEvent>(), It.IsAny<DeliveryOptions>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTemplateNotFound()
    {
        var fileId = Guid.NewGuid();
        _dbContext.Files.Add(
            new LogsheetXtractor.Domain.Entities.File
            {
                Id = fileId,
                OriginalFileName = "f.jpg",
                StoragePath = "p",
            }
        );
        await _dbContext.SaveChangesAsync();

        var command = new CreateLogsheetCommand(Guid.NewGuid(), null, fileId);

        var result = await CreateLogsheetHandler.Handle(
            command,
            CancellationToken.None,
            _dbContext,
            _mapperMock.Object,
            _busMock.Object
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Template not found");
        _busMock.Verify(
            b => b.PublishAsync(It.IsAny<LogsheetCreatedEvent>(), It.IsAny<DeliveryOptions>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldPublishEventWithDisabledAutomaticAlignment_WhenRequested()
    {
        var templateId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        var templateFile = new LogsheetXtractor.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "template.pdf",
            StoredFileName = "template.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };
        _dbContext.Files.Add(templateFile);

        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = templateId,
            Name = "Template",
            FileId = templateFile.Id,
        };
        var file = new LogsheetXtractor.Domain.Entities.File
        {
            Id = fileId,
            OriginalFileName = "logsheet.jpg",
            StoragePath = "path",
        };
        _dbContext.Templates.Add(template);
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        var command = new CreateLogsheetCommand(templateId, null, fileId, false);
        var mappedLogsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            FileId = fileId,
            Template = template,
            File = file,
            Status = ELogSheetStatus.Pending,
        };

        _mapperMock.Setup(m => m.Map<Logsheet>(command)).Returns(mappedLogsheet);
        _mapperMock
            .Setup(m => m.Map<LogsheetDetailDto>(It.IsAny<Logsheet>()))
            .Returns(
                new LogsheetDetailDto(
                    mappedLogsheet.Id,
                    new TemplateListDto(
                        templateId,
                        "Template",
                        null,
                        null,
                        null,
                        0,
                        0,
                        0,
                        0,
                        DateTime.UtcNow
                    ),
                    new FileDto(fileId, "logsheet.jpg", "image/jpeg", 100, DateTime.UtcNow),
                    ELogSheetStatus.Pending,
                    null,
                    null,
                    new List<ExtractedValueDto>(),
                    DateTime.UtcNow,
                    null
                )
            );

        var result = await CreateLogsheetHandler.Handle(
            command,
            CancellationToken.None,
            _dbContext,
            _mapperMock.Object,
            _busMock.Object
        );

        result.IsSuccess.Should().BeTrue();
        _busMock.Verify(
            b =>
                b.PublishAsync(
                    It.Is<LogsheetCreatedEvent>(e =>
                        e.LogsheetId == mappedLogsheet.Id && !e.PerformAutomaticAlignment
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
