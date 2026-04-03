using FluentAssertions;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.File.Interfaces;
using LogsheetXtractor.Application.Features.Logsheets;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LogsheetXtractor.UnitTests.Application.Features.Logsheets;

public class DeleteBatchLogsheetHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IFileService> _fileServiceMock = new();

    [Fact]
    public async Task Handle_ShouldDeleteAllRequestedLogsheets_WhenAllExist()
    {
        var templateFile = CreateFile("template.pdf");
        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Name = "Template",
            File = templateFile,
            FileId = templateFile.Id,
        };

        var file1 = CreateFile("logsheet-1.pdf");
        var file2 = CreateFile("logsheet-2.pdf");
        var logsheet1 = new Logsheet
        {
            Template = template,
            TemplateId = template.Id,
            File = file1,
            FileId = file1.Id,
            Status = ELogSheetStatus.Pending,
        };
        var logsheet2 = new Logsheet
        {
            Template = template,
            TemplateId = template.Id,
            File = file2,
            FileId = file2.Id,
            Status = ELogSheetStatus.Pending,
        };

        _dbContext.Templates.Add(template);
        _dbContext.Logsheets.AddRange(logsheet1, logsheet2);
        await _dbContext.SaveChangesAsync();

        var command = new BatchDeleteLogsheetCommand([logsheet1.Id, logsheet2.Id]);

        var result = await DeleteBatchLogsheetHandler.Handle(
            command,
            _fileServiceMock.Object,
            CancellationToken.None,
            _dbContext
        );

        result.IsSuccess.Should().BeTrue();

        var deletedLogsheets = await _dbContext
            .Logsheets.IgnoreQueryFilters()
            .Where(l => l.Id == logsheet1.Id || l.Id == logsheet2.Id)
            .ToListAsync();

        deletedLogsheets.Should().HaveCount(2);
        deletedLogsheets.Should().OnlyContain(l => l.DeletedAt != null);

        _fileServiceMock.Verify(
            fs =>
                fs.DeleteFilesAsync(
                    It.Is<IEnumerable<Guid>>(ids =>
                        ids.Contains(logsheet1.FileId)
                        && ids.Contains(logsheet2.FileId)
                        && ids.Count() == 2
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenAtLeastOneLogsheetDoesNotExist()
    {
        var templateFile = CreateFile("template.pdf");
        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Name = "Template",
            File = templateFile,
            FileId = templateFile.Id,
        };

        var file = CreateFile("logsheet.pdf");
        var existingLogsheet = new Logsheet
        {
            Template = template,
            TemplateId = template.Id,
            File = file,
            FileId = file.Id,
            Status = ELogSheetStatus.Pending,
        };
        _dbContext.Templates.Add(template);
        _dbContext.Logsheets.Add(existingLogsheet);
        await _dbContext.SaveChangesAsync();

        var command = new BatchDeleteLogsheetCommand([existingLogsheet.Id, Guid.NewGuid()]);

        var result = await DeleteBatchLogsheetHandler.Handle(
            command,
            _fileServiceMock.Object,
            CancellationToken.None,
            _dbContext
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
        _fileServiceMock.Verify(
            fs => fs.DeleteFilesAsync(It.IsAny<IEnumerable<Guid>>()),
            Times.Never
        );
    }

    private static LogsheetXtractor.Domain.Entities.File CreateFile(string fileName)
    {
        return new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = fileName,
            StoredFileName = fileName,
            StoragePath = $"storage/{fileName}",
            ContentType = "application/pdf",
            SizeBytes = 120,
        };
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
