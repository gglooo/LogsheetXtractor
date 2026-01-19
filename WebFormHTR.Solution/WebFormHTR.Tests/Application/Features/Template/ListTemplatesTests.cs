using FluentAssertions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Features.Template;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;

namespace WebFormHTR.Tests.Application.Features.Template;

public class ListTemplatesTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IMapper> _mapperMock = new();

    [Fact]
    public async Task Handle_ShouldReturnAllTemplates_WhenSearchIsEmpty()
    {
        var file = new Domain.Entities.File
        {
            OriginalFileName = "test.pdf", StoredFileName = "test.pdf", StoragePath = "path",
            ContentType = "application/pdf"
        };
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        var templates = new List<Domain.Entities.Template>
        {
            new() { Name = "Template 1", FileId = file.Id },
            new() { Name = "Template 2", FileId = file.Id }
        };
        _dbContext.Templates.AddRange(templates);
        await _dbContext.SaveChangesAsync();

        var query = new ListTemplatesQuery(null);

        var expectedDtos = new List<TemplateListDto>
        {
            new(Guid.NewGuid().ToString(), "Template 1", null, null, 0, 0, 0, DateTime.UtcNow),
            new(Guid.NewGuid().ToString(), "Template 2", null, null, 0, 0, 0, DateTime.UtcNow)
        };

        _mapperMock.Setup(m => m.Map<IEnumerable<TemplateListDto>>(It.IsAny<List<Domain.Entities.Template>>()))
            .Returns(expectedDtos);

        var result = await ListTemplatesHandler.Handle(query, _dbContext, _mapperMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}