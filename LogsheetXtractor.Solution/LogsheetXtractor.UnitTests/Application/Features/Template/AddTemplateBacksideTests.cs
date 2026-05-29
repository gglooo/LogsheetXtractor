using FluentAssertions;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Template;
using LogsheetXtractor.Application.Features.Template.DTOs;
using LogsheetXtractor.Application.Features.Template.Interfaces;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using Moq;

namespace LogsheetXtractor.UnitTests.Application.Features.Template;

public class AddTemplateBacksideTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<ITemplateService> _templateServiceMock = new();

    [Fact]
    public async Task Handle_ShouldReturnError_WhenTemplateNotFound()
    {
        var command = new AddTemplateBacksideCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = await AddTemplateBacksideHandler.Handle(
            command,
            _templateServiceMock.Object,
            _dbContext,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
        result.Errors.First().Message.Should().Be("Template not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenTemplateAlreadyHasBackside()
    {
        var frontFile = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "front.pdf",
            StoredFileName = "front.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };
        var backFile = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "back.pdf",
            StoredFileName = "back.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };

        _dbContext.Files.AddRange(frontFile, backFile);
        await _dbContext.SaveChangesAsync();

        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = Guid.NewGuid(),
            Name = "Front",
            FileId = frontFile.Id,
        };
        var backside = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = Guid.NewGuid(),
            Name = "Back",
            FileId = backFile.Id,
        };
        template.SetBacksideTemplate(backside);

        _dbContext.Templates.AddRange(template, backside);
        await _dbContext.SaveChangesAsync();

        var command = new AddTemplateBacksideCommand(template.Id,  backFile.Id);

        var result = await AddTemplateBacksideHandler.Handle(
            command,
            _templateServiceMock.Object,
            _dbContext,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<InvalidStateError>();
        result.Errors.First().Message.Should().Be("Template already has a backside");
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenTemplateIsNotEditable()
    {
        var templateFile = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "front.pdf",
            StoredFileName = "front.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };
        var logsheetFile = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "logsheet.pdf",
            StoredFileName = "logsheet.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };
        var backsideFile = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "back.pdf",
            StoredFileName = "back.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };

        _dbContext.Files.AddRange(templateFile, logsheetFile, backsideFile);
        await _dbContext.SaveChangesAsync();

        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Name = "Front",
            FileId = templateFile.Id,
        };
        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();

        var logsheet = new Logsheet
        {
            TemplateId = template.Id,
            FileId = logsheetFile.Id,
            Status = ELogSheetStatus.Completed,
        };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        var command = new AddTemplateBacksideCommand(template.Id,  backsideFile.Id);

        var result = await AddTemplateBacksideHandler.Handle(
            command,
            _templateServiceMock.Object,
            _dbContext,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<InvalidStateError>();
        result.Errors.First().Message.Should().Be("Template is not editable");
    }

    [Fact]
    public async Task Handle_ShouldCallService_WhenRequestIsValid()
    {
        var frontFile = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "front.pdf",
            StoredFileName = "front.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };
        var backFile = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "back.pdf",
            StoredFileName = "back.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };

        _dbContext.Files.AddRange(frontFile, backFile);
        await _dbContext.SaveChangesAsync();

        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Name = "Front",
            FileId = frontFile.Id,
        };
        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();

        var expectedDto = new TemplateDetailDto(
            template.Id,
            template.Name,
            0,
            0,
            null,
            null,
            null,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow,
            [],
            [],
            true
        );

        _templateServiceMock
            .Setup(s =>
                s.AddBacksideTemplateAsync(
                    template.Id,
                    backFile.Id,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(expectedDto);

        var command = new AddTemplateBacksideCommand(template.Id, backFile.Id);

        var result = await AddTemplateBacksideHandler.Handle(
            command,
            _templateServiceMock.Object,
            _dbContext,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDto);
        _templateServiceMock.Verify(
            s =>
                s.AddBacksideTemplateAsync(
                    template.Id,
                    backFile.Id,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
