using FluentAssertions;
using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Application.Features.Logsheets;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Infrastructure.Persistence;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.Logsheets;

public class CreateLogsheetCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IMapper> _mapperMock;

    public CreateLogsheetCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new AppDbContext(options);
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task Handle_ShouldCreateLogsheet_WhenRequestIsValid()
    {
        var templateId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        
        var template = new Domain.Entities.Template { Id = templateId, Name = "Test Template" };
        var file = new Domain.Entities.File { Id = fileId, OriginalFileName = "test.jpg", StoragePath = "path/to/file" };
        
        _dbContext.Templates.Add(template);
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        var command = new CreateLogsheetCommand(templateId, fileId);
        
        var templateDto = new TemplateListDto(templateId.ToString(), "Test Template", null, null);
        var fileDto = new FileDto(fileId, "test.jpg", "image/jpeg", 100, DateTime.UtcNow);
        var expectedDto = new LogsheetDetailDto(Guid.NewGuid(), templateDto, fileDto, ELogSheetStatus.Pending, DateTime.UtcNow);

        _mapperMock.Setup(x => x.Map<Domain.Entities.Logsheet>(command))
            .Returns(new Domain.Entities.Logsheet { Id = expectedDto.Id, TemplateId = templateId, FileId = fileId, Template = null!, File = null! });
            
        _mapperMock.Setup(x => x.Map<LogsheetDetailDto>(It.IsAny<Domain.Entities.Logsheet>()))
            .Returns(expectedDto);

        var result = await CreateLogsheetHandler.Handle(command, CancellationToken.None, _dbContext, _mapperMock.Object);

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
        
        var template = new Domain.Entities.Template { Id = templateId, Name = "Test Template" };
        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();

        var command = new CreateLogsheetCommand(templateId, fileId);

        var result = await CreateLogsheetHandler.Handle(command, CancellationToken.None, _dbContext, _mapperMock.Object);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "File not found");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTemplate_NotFound()
    {
        var templateId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        
        var file = new Domain.Entities.File { Id = fileId, OriginalFileName = "test.jpg", StoragePath = "path/to/file" };
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        var command = new CreateLogsheetCommand(templateId, fileId);

        var result = await CreateLogsheetHandler.Handle(command, CancellationToken.None, _dbContext, _mapperMock.Object);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Template not found");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
