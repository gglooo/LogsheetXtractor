using FluentAssertions;
using LogsheetXtractor.Application.Features.File.Interfaces;
using LogsheetXtractor.Application.Features.Template;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LogsheetXtractor.UnitTests.Application.Features.Template;

public class DeleteTemplateCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IFileService> _fileServiceMock = new();

    [Fact]
    public async Task Handle_ShouldDeleteTemplateAndBackside_WhenDeletingFrontside()
    {
        var templateId = Guid.NewGuid();
        var templateFileId = Guid.NewGuid();
        var backsideId = Guid.NewGuid();
        var backsideFileId = Guid.NewGuid();

        var backside = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = backsideId,
            FileId = backsideFileId,
            File = null!,
        };

        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = templateId,
            FileId = templateFileId,
            File = null!,
        };

        template.ForceSetBacksideTemplate(backside);

        _dbContext.Templates.AddRange(template, backside);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteTemplateCommand(templateId);

        var result = await DeleteTemplateHandler.Handle(
            command,
            _dbContext,
            _fileServiceMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();

        var deletedTemplates = await _dbContext
            .Templates.IgnoreQueryFilters()
            .Where(t => t.Id == templateId || t.Id == backsideId)
            .ToListAsync();

        deletedTemplates.Should().HaveCount(2);
        deletedTemplates.Should().OnlyContain(t => t.DeletedAt.HasValue);

        _fileServiceMock.Verify(
            x =>
                x.DeleteFilesAsync(
                    It.Is<IEnumerable<Guid>>(ids =>
                        ids.Contains(templateFileId)
                        && ids.Contains(backsideFileId)
                        && ids.Count() == 2
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldDeleteTemplateAndBackside_WhenDeletingBackside()
    {
        var templateId = Guid.NewGuid();
        var templateFileId = Guid.NewGuid();
        var backsideId = Guid.NewGuid();
        var backsideFileId = Guid.NewGuid();

        var backside = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = backsideId,
            FileId = backsideFileId,
            File = null!,
        };

        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = templateId,
            FileId = templateFileId,
            File = null!,
        };

        template.ForceSetBacksideTemplate(backside);

        _dbContext.Templates.AddRange(template, backside);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteTemplateCommand(backsideId);

        var result = await DeleteTemplateHandler.Handle(
            command,
            _dbContext,
            _fileServiceMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();

        var deletedTemplates = await _dbContext
            .Templates.IgnoreQueryFilters()
            .Where(t => t.Id == templateId || t.Id == backsideId)
            .ToListAsync();

        deletedTemplates.Should().HaveCount(2);
        deletedTemplates.Should().OnlyContain(t => t.DeletedAt.HasValue);

        _fileServiceMock.Verify(
            x =>
                x.DeleteFilesAsync(
                    It.Is<IEnumerable<Guid>>(ids =>
                        ids.Contains(templateFileId)
                        && ids.Contains(backsideFileId)
                        && ids.Count() == 2
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTemplate_NotFound()
    {
        var command = new DeleteTemplateCommand(Guid.NewGuid());

        var result = await DeleteTemplateHandler.Handle(
            command,
            _dbContext,
            _fileServiceMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Template not found");
        _fileServiceMock.Verify(
            x => x.DeleteFilesAsync(It.IsAny<IEnumerable<Guid>>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldSoftDeleteDependentLogsheets_WhenTemplateIsDeleted()
    {
        var templateFileId = Guid.NewGuid();
        var logsheetFileId = Guid.NewGuid();
        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = Guid.NewGuid(),
            FileId = templateFileId,
            File = null!,
        };
        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            Template = template,
            FileId = logsheetFileId,
            Status = ELogSheetStatus.Completed,
        };

        _dbContext.Templates.Add(template);
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        var result = await DeleteTemplateHandler.Handle(
            new DeleteTemplateCommand(template.Id),
            _dbContext,
            _fileServiceMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();

        var deletedLogsheet = await _dbContext
            .Logsheets.IgnoreQueryFilters()
            .SingleAsync(l => l.Id == logsheet.Id);
        deletedLogsheet.DeletedAt.Should().NotBeNull();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
