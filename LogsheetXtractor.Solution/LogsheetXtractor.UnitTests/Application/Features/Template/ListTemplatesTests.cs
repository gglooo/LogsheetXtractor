using FluentAssertions;
using LogsheetXtractor.Application.Features.Template;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Domain.ValueObjects;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;

namespace LogsheetXtractor.UnitTests.Application.Features.Template;

public class ListTemplatesTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();

    [Fact]
    public async Task Handle_ShouldReturnAllTemplates_WhenSearchIsEmpty()
    {
        var file1 = CreateFile("test.pdf");
        var file2 = CreateFile("sample.pdf");
        _dbContext.Files.AddRange(file1, file2);
        await _dbContext.SaveChangesAsync();

        var templates = new List<LogsheetXtractor.Domain.Entities.Template>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Template 1",
                FileId = file1.Id,
                Width = 10,
                Height = 10,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Template 2",
                FileId = file2.Id,
                Width = 10,
                Height = 10,
            },
        };
        _dbContext.Templates.AddRange(templates);
        await _dbContext.SaveChangesAsync();

        var query = new ListTemplatesQuery(null);

        var result = await ListTemplatesHandler.Handle(query, _dbContext);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldIncludeRoiCountFromFrontsideAndBackside()
    {
        var frontFile = CreateFile("front.pdf");
        var backFile = CreateFile("back.pdf");
        _dbContext.Files.AddRange(frontFile, backFile);
        await _dbContext.SaveChangesAsync();

        var frontTemplate = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = Guid.NewGuid(),
            Name = "Front Template",
            FileId = frontFile.Id,
            Width = 10,
            Height = 10,
        };
        var backTemplate = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = Guid.NewGuid(),
            Name = "Back Template",
            FileId = backFile.Id,
            Width = 10,
            Height = 10,
        };
        frontTemplate.SetBacksideTemplate(backTemplate);

        _dbContext.Templates.AddRange(frontTemplate, backTemplate);
        await _dbContext.SaveChangesAsync();

        _dbContext.Rois.AddRange(
            new LogsheetXtractor.Domain.Entities.Roi
            {
                Id = Guid.NewGuid(),
                TemplateId = frontTemplate.Id,
                VariableName = "front_1",
                Type = ERoiType.Handwritten,
                Coordinates = new Coordinates(0, 0, 10, 10),
            },
            new LogsheetXtractor.Domain.Entities.Roi
            {
                Id = Guid.NewGuid(),
                TemplateId = frontTemplate.Id,
                VariableName = "front_2",
                Type = ERoiType.Handwritten,
                Coordinates = new Coordinates(0, 0, 10, 10),
            },
            new LogsheetXtractor.Domain.Entities.Roi
            {
                Id = Guid.NewGuid(),
                TemplateId = backTemplate.Id,
                VariableName = "back_1",
                Type = ERoiType.Handwritten,
                Coordinates = new Coordinates(0, 0, 10, 10),
            }
        );
        await _dbContext.SaveChangesAsync();

        var result = await ListTemplatesHandler.Handle(new ListTemplatesQuery(null), _dbContext);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value.Single().RoiCount.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ShouldExcludeSoftDeletedTemplates()
    {
        var activeFile = CreateFile("active.pdf");
        var deletedFile = CreateFile("deleted.pdf");
        _dbContext.Files.AddRange(activeFile, deletedFile);
        await _dbContext.SaveChangesAsync();

        var activeTemplate = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = Guid.NewGuid(),
            Name = "Active Template",
            FileId = activeFile.Id,
            Width = 10,
            Height = 10,
        };
        var deletedTemplate = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = Guid.NewGuid(),
            Name = "Deleted Template",
            FileId = deletedFile.Id,
            Width = 10,
            Height = 10,
            DeletedAt = DateTime.UtcNow,
        };

        _dbContext.Templates.AddRange(activeTemplate, deletedTemplate);
        await _dbContext.SaveChangesAsync();

        var result = await ListTemplatesHandler.Handle(new ListTemplatesQuery(null), _dbContext);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value.Single().Id.Should().Be(activeTemplate.Id);
    }

    private static LogsheetXtractor.Domain.Entities.File CreateFile(string fileName)
    {
        return new LogsheetXtractor.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            OriginalFileName = fileName,
            StoredFileName = fileName,
            StoragePath = $"path/{fileName}",
            ContentType = "application/pdf",
        };
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
