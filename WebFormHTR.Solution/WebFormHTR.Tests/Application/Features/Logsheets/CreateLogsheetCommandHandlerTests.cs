using FluentAssertions;
using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Application.Features.ExtractedValues.DTOs;
using WebFormHTR.Application.Features.Logsheets;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.Logsheets;

public class CreateLogsheetCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IMapper> _mapperMock;

    public CreateLogsheetCommandHandlerTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task Handle_ShouldCreateLogsheet_WhenRequestIsValid()
    {
        var templateId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        var templateFile = new Domain.Entities.File
        {
            Id = Guid.NewGuid(), OriginalFileName = "template.pdf", StoredFileName = "template.pdf",
            StoragePath = "path", ContentType = "application/pdf"
        };
        _dbContext.Files.Add(templateFile);

        var template = new Domain.Entities.Template
            { Id = templateId, Name = "Test Template", FileId = templateFile.Id };
        var file = new Domain.Entities.File
            { Id = fileId, OriginalFileName = "test.jpg", StoragePath = "path/to/file" };

        _dbContext.Templates.Add(template);
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        var command = new CreateLogsheetCommand(templateId, null, fileId);

        var templateDto = new TemplateListDto(templateId.ToString(), "Test Template", null, null);
        var fileDto = new FileDto(fileId, "test.jpg", "image/jpeg", 100, DateTime.UtcNow);
        var expectedDto = new LogsheetDetailDto(Guid.NewGuid(), templateDto, null!, fileDto, ELogSheetStatus.Pending,
            DateTime.UtcNow, null, new List<ExtractedValueDto>());

        _mapperMock.Setup(x => x.Map<Logsheet>(command))
            .Returns(new Logsheet
                { Id = expectedDto.Id, TemplateId = templateId, FileId = fileId, Template = null!, File = null! });

        _mapperMock.Setup(x => x.Map<LogsheetDetailDto>(It.IsAny<Logsheet>()))
            .Returns(expectedDto);

        var result =
            await CreateLogsheetHandler.Handle(command, CancellationToken.None, _dbContext, _mapperMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);

        var savedLogsheet = await _dbContext.Logsheets.FirstOrDefaultAsync();
        savedLogsheet.Should().NotBeNull();
        savedLogsheet!.TemplateId.Should().Be(templateId);
        savedLogsheet.FileId.Should().Be(fileId);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenFile_NotFound()
    {
        var templateId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        var templateFile = new Domain.Entities.File
        {
            Id = Guid.NewGuid(), OriginalFileName = "template.pdf", StoredFileName = "template.pdf",
            StoragePath = "path", ContentType = "application/pdf"
        };
        _dbContext.Files.Add(templateFile);

        var template = new Domain.Entities.Template
            { Id = templateId, Name = "Test Template", FileId = templateFile.Id };
        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();

        var command = new CreateLogsheetCommand(templateId, null, fileId);

        var result =
            await CreateLogsheetHandler.Handle(command, CancellationToken.None, _dbContext, _mapperMock.Object);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "File not found");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTemplate_NotFound()
    {
        var templateId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        var file = new Domain.Entities.File
            { Id = fileId, OriginalFileName = "test.jpg", StoragePath = "path/to/file" };
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        var command = new CreateLogsheetCommand(templateId, null, fileId);

        var result =
            await CreateLogsheetHandler.Handle(command, CancellationToken.None, _dbContext, _mapperMock.Object);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Template not found");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}