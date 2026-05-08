using FluentAssertions;
using LogsheetXtractor.Application.Features.File.Interfaces;
using LogsheetXtractor.Application.Features.Template;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using Moq;

namespace LogsheetXtractor.UnitTests.Application.Features.Template;

public class GetTemplatePreviewTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IFileService> _fileServiceMock = new();

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenTemplateDoesNotExist()
    {
        var result = await GetTemplatePreviewHandler.HandleAsync(
            new GetTemplatePreviewQuery(Guid.NewGuid()),
            _dbContext,
            _fileServiceMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Template not found");
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenPreviewIsMissing()
    {
        var template = await SeedTemplateAsync("missing-preview-template");

        _fileServiceMock
            .Setup(s => s.GetFilePreviewAsync(template.FileId))
            .ReturnsAsync((LogsheetXtractor.Application.DTOs.GetFileDto?)null);

        var result = await GetTemplatePreviewHandler.HandleAsync(
            new GetTemplatePreviewQuery(template.Id),
            _dbContext,
            _fileServiceMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Template preview not available.");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnPreview_WhenAvailable()
    {
        var template = await SeedTemplateAsync("preview-template");
        var preview = new LogsheetXtractor.Application.DTOs.GetFileDto
        {
            FileName = "template-preview.png",
            ContentType = "image/png",
            Stream = new MemoryStream([1, 2, 3]),
        };

        _fileServiceMock
            .Setup(s => s.GetFilePreviewAsync(template.FileId))
            .ReturnsAsync(preview);

        var result = await GetTemplatePreviewHandler.HandleAsync(
            new GetTemplatePreviewQuery(template.Id),
            _dbContext,
            _fileServiceMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(preview);
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenUnexpectedExceptionOccurs()
    {
        var template = await SeedTemplateAsync("exception-template");

        _fileServiceMock
            .Setup(s => s.GetFilePreviewAsync(template.FileId))
            .ThrowsAsync(new InvalidOperationException("Preview service failure"));

        var result = await GetTemplatePreviewHandler.HandleAsync(
            new GetTemplatePreviewQuery(template.Id),
            _dbContext,
            _fileServiceMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("Preview service failure"));
    }

    private async Task<LogsheetXtractor.Domain.Entities.Template> SeedTemplateAsync(string templateName)
    {
        var file = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = $"{templateName}.pdf",
            StoredFileName = $"{templateName}.pdf",
            StoragePath = $"storage/{templateName}.pdf",
            ContentType = "application/pdf",
            SizeBytes = 32,
        };

        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Name = templateName,
            FileId = file.Id,
            File = file,
        };

        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();
        return template;
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
