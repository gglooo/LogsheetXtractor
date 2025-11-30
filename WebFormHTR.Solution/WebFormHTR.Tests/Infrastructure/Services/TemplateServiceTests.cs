using FluentAssertions;
using MapsterMapper;
using Moq;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Infrastructure.Services;
using WebFormHTR.Tests.Common;

namespace WebFormHTR.Tests.Infrastructure.Services;

public class TemplateServiceTests
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IMapper> _mapperMock;
    private readonly TemplateService _templateService;

    public TemplateServiceTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mapperMock = new Mock<IMapper>();
        _templateService = new TemplateService(_dbContext, _mapperMock.Object);
    }

    [Fact]
    public async Task CloneTemplateAsync_ShouldCloneTemplate_WhenParentExists()
    {
        var parentFile = new Domain.Entities.File { OriginalFileName = "parent.pdf", StoredFileName = "parent.pdf", StoragePath = "path", ContentType = "application/pdf" };
        _dbContext.Files.Add(parentFile);
        await _dbContext.SaveChangesAsync();

        var parentId = Guid.NewGuid();
        var parentTemplate = new Template { Id = parentId, Name = "Parent Template", FileId = parentFile.Id };
        _dbContext.Templates.Add(parentTemplate);
        await _dbContext.SaveChangesAsync();

        var newFile = new Domain.Entities.File { OriginalFileName = "new.pdf", StoredFileName = "new.pdf", StoragePath = "path", ContentType = "application/pdf" };
        _dbContext.Files.Add(newFile);
        await _dbContext.SaveChangesAsync();

        var newTemplateName = "Cloned Template";
        var fileId = newFile.Id;

        var expectedDto = new TemplateDetailDto(
            Guid.NewGuid(),
            newTemplateName,
            null,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow,
            []
        );
        _mapperMock.Setup(x => x.Map<TemplateDetailDto>(It.IsAny<Template>())).Returns(expectedDto);

        var result = await _templateService.CloneTemplateAsync(
            parentId,
            newTemplateName,
            fileId,
            CancellationToken.None
        );
        await _dbContext.SaveChangesAsync();

        result.Should().Be(expectedDto);
        _dbContext
            .Templates.Should()
            .Contain(t =>
                t.Name == newTemplateName && t.ParentId == parentId && t.FileId == fileId
            );
    }
}