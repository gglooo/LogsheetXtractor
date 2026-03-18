using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Application.Features.ExtractedValues.DTOs;
using WebFormHTR.Application.Features.Logsheets;
using WebFormHTR.Application.Features.Logsheets.Create;
using WebFormHTR.Application.Features.Logsheets.Create.Events;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Features.Template.DTOs;
using DomainTemplate = WebFormHTR.Domain.Entities.Template;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;
using Wolverine;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.Logsheets;

public class CreateBatchLogsheetsTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<BatchCreateLogsheetCommand>> _loggerMock;
    private readonly Mock<IMessageBus> _busMock;

    public CreateBatchLogsheetsTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<BatchCreateLogsheetCommand>>();
        _busMock = new Mock<IMessageBus>();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenFileIdsListIsEmpty()
    {
        var command = new BatchCreateLogsheetCommand(Guid.NewGuid(), null, Array.Empty<Guid>());

        var result = await CreateBatchLogsheetsHandler.Handle(command, CancellationToken.None, _dbContext,
            _mapperMock.Object, _busMock.Object, _loggerMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenAnyFileNotFound()
    {
        var fileId = Guid.NewGuid();
        var command = new BatchCreateLogsheetCommand(Guid.NewGuid(), null, new[] { fileId });

        var result = await CreateBatchLogsheetsHandler.Handle(command, CancellationToken.None, _dbContext,
            _mapperMock.Object, _busMock.Object, _loggerMock.Object);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "One or more files not found");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenAnyFileAlreadyAssigned()
    {
        var fileId = Guid.NewGuid();
        var file = newDomainFile(fileId);
        _dbContext.Files.Add(file);

        var logsheet = new Logsheet
            { Id = Guid.NewGuid(), FileId = fileId, TemplateId = Guid.NewGuid(), Template = null!, File = null! };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        var command = new BatchCreateLogsheetCommand(Guid.NewGuid(), null, new[] { fileId });

        var result = await CreateBatchLogsheetsHandler.Handle(command, CancellationToken.None, _dbContext,
            _mapperMock.Object, _busMock.Object, _loggerMock.Object);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "One or more files are already assigned to a logsheet");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTemplateNotFound()
    {
        var fileId = Guid.NewGuid();
        var file = newDomainFile(fileId);
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        var command = new BatchCreateLogsheetCommand(Guid.NewGuid(), null, new[] { fileId });

        var result = await CreateBatchLogsheetsHandler.Handle(command, CancellationToken.None, _dbContext,
            _mapperMock.Object, _busMock.Object, _loggerMock.Object);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "One or more templates not found");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenBacksideTemplateNotFound()
    {
        var templateId = Guid.NewGuid();
        var templateFile = newDomainFile(Guid.NewGuid());
        _dbContext.Files.Add(templateFile);
        var template = new DomainTemplate { Id = templateId, Name = "Template", FileId = templateFile.Id };
        _dbContext.Templates.Add(template);

        var fileId = Guid.NewGuid();
        var file = newDomainFile(fileId);
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        var command = new BatchCreateLogsheetCommand(templateId, Guid.NewGuid(), new[] { fileId });

        var result = await CreateBatchLogsheetsHandler.Handle(command, CancellationToken.None, _dbContext,
            _mapperMock.Object, _busMock.Object, _loggerMock.Object);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "One or more templates not found");
    }

    [Fact]
    public async Task Handle_ShouldCreateLogsheets_WhenRequestIsValid()
    {
        var templateId = Guid.NewGuid();
        var templateFile = newDomainFile(Guid.NewGuid());
        _dbContext.Files.Add(templateFile);
        var template = new DomainTemplate { Id = templateId, Name = "Template", FileId = templateFile.Id };
        _dbContext.Templates.Add(template);

        var fileId1 = Guid.NewGuid();
        var file1 = newDomainFile(fileId1);
        _dbContext.Files.Add(file1);

        var fileId2 = Guid.NewGuid();
        var file2 = newDomainFile(fileId2);
        _dbContext.Files.Add(file2);

        await _dbContext.SaveChangesAsync();

        var command = new BatchCreateLogsheetCommand(templateId, null, new[] { fileId1, fileId2 });

        var logsheet1 = new Logsheet
            { Id = Guid.NewGuid(), TemplateId = templateId, FileId = fileId1, Template = null!, File = null! };
        var logsheet2 = new Logsheet
            { Id = Guid.NewGuid(), TemplateId = templateId, FileId = fileId2, Template = null!, File = null! };

        _mapperMock.Setup(m => m.Map<IList<Logsheet>>(It.IsAny<IEnumerable<CreateLogsheetCommand>>()))
            .Returns(new List<Logsheet> { logsheet1, logsheet2 });

        var expectedDtos = new List<LogsheetDetailDto>();
        _mapperMock.Setup(m => m.Map<IEnumerable<LogsheetDetailDto>>(It.IsAny<List<Logsheet>>()))
            .Returns(expectedDtos);

        var result = await CreateBatchLogsheetsHandler.Handle(command, CancellationToken.None, _dbContext,
            _mapperMock.Object, _busMock.Object, _loggerMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDtos);

        var savedLogsheets = await _dbContext.Logsheets.ToListAsync();
        savedLogsheets.Should().HaveCount(2);
        savedLogsheets.Should().Contain(l => l.FileId == fileId1 && l.TemplateId == templateId);
        savedLogsheets.Should().Contain(l => l.FileId == fileId2 && l.TemplateId == templateId);

        _busMock.Verify(
            b => b.PublishAsync(
                It.Is<LogsheetCreatedEvent>(e => e.PerformAutomaticAlignment),
                It.IsAny<DeliveryOptions>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldCreateLogsheets_WhenRequestIsValid_WithBackside()
    {
        var templateId = Guid.NewGuid();
        var templateFile = newDomainFile(Guid.NewGuid());
        _dbContext.Files.Add(templateFile);
        var template = new DomainTemplate { Id = templateId, Name = "Template", FileId = templateFile.Id };
        _dbContext.Templates.Add(template);

        var backsideTemplateId = Guid.NewGuid();
        var backsideTemplateFile = newDomainFile(Guid.NewGuid());
        _dbContext.Files.Add(backsideTemplateFile);
        var backsideTemplate = new DomainTemplate
            { Id = backsideTemplateId, Name = "Backside", FileId = backsideTemplateFile.Id };
        _dbContext.Templates.Add(backsideTemplate);

        var fileId = Guid.NewGuid();
        var file = newDomainFile(fileId);
        _dbContext.Files.Add(file);

        await _dbContext.SaveChangesAsync();

        var command = new BatchCreateLogsheetCommand(templateId, backsideTemplateId, new[] { fileId });

        var logsheet = new Logsheet
            { Id = Guid.NewGuid(), TemplateId = templateId, FileId = fileId, Template = null!, File = null! };

        _mapperMock.Setup(m => m.Map<IList<Logsheet>>(It.IsAny<IEnumerable<CreateLogsheetCommand>>()))
            .Returns(new List<Logsheet> { logsheet });

        var expectedDtos = new List<LogsheetDetailDto>();
        _mapperMock.Setup(m => m.Map<IEnumerable<LogsheetDetailDto>>(It.IsAny<List<Logsheet>>()))
            .Returns(expectedDtos);

        var result = await CreateBatchLogsheetsHandler.Handle(command, CancellationToken.None, _dbContext,
            _mapperMock.Object, _busMock.Object, _loggerMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDtos);

        var savedLogsheets = await _dbContext.Logsheets.ToListAsync();
        savedLogsheets.Should().HaveCount(1);
        savedLogsheets.First().TemplateId.Should().Be(templateId);

        _busMock.Verify(
            b => b.PublishAsync(
                It.Is<LogsheetCreatedEvent>(e => e.PerformAutomaticAlignment),
                It.IsAny<DeliveryOptions>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPublishEventsWithDisabledAutomaticAlignment_WhenRequested()
    {
        var templateId = Guid.NewGuid();
        var templateFile = newDomainFile(Guid.NewGuid());
        _dbContext.Files.Add(templateFile);
        var template = new DomainTemplate { Id = templateId, Name = "Template", FileId = templateFile.Id };
        _dbContext.Templates.Add(template);

        var fileId = Guid.NewGuid();
        var file = newDomainFile(fileId);
        _dbContext.Files.Add(file);

        await _dbContext.SaveChangesAsync();

        var command = new BatchCreateLogsheetCommand(templateId, null, new[] { fileId }, false);

        var logsheet = new Logsheet
            { Id = Guid.NewGuid(), TemplateId = templateId, FileId = fileId, Template = null!, File = null! };

        _mapperMock.Setup(m => m.Map<IList<Logsheet>>(It.IsAny<IEnumerable<CreateLogsheetCommand>>()))
            .Returns(new List<Logsheet> { logsheet });

        var expectedDtos = new List<LogsheetDetailDto>();
        _mapperMock.Setup(m => m.Map<IEnumerable<LogsheetDetailDto>>(It.IsAny<List<Logsheet>>()))
            .Returns(expectedDtos);

        var result = await CreateBatchLogsheetsHandler.Handle(command, CancellationToken.None, _dbContext,
            _mapperMock.Object, _busMock.Object, _loggerMock.Object);

        result.IsSuccess.Should().BeTrue();
        _busMock.Verify(
            b => b.PublishAsync(
                It.Is<LogsheetCreatedEvent>(e =>
                    e.LogsheetId == logsheet.Id && !e.PerformAutomaticAlignment),
                It.IsAny<DeliveryOptions>()),
            Times.Once);
    }

    private Domain.Entities.File newDomainFile(Guid id)
    {
        return new Domain.Entities.File
        {
            Id = id,
            OriginalFileName = "test.txt",
            StoredFileName = "test.txt",
            StoragePath = "/tmp/test.txt",
            ContentType = "text/plain"
        };
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
