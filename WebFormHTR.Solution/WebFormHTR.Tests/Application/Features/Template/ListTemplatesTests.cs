using FluentAssertions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Features.Template;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Infrastructure.Persistence;

namespace WebFormHTR.Tests.Application.Features.Template;

public class ListTemplatesTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IMapper> _mapperMock;

    public ListTemplatesTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new AppDbContext(options);
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task Handle_ShouldReturnAllTemplates_WhenSearchIsEmpty()
    {
        var templates = new List<Domain.Entities.Template>
        {
            new() { Name = "Template 1" },
            new() { Name = "Template 2" }
        };
        _dbContext.Templates.AddRange(templates);
        await _dbContext.SaveChangesAsync();

        var query = new ListTemplatesQuery(null);
        
        var expectedDtos = new List<TemplateListDto>
        {
            new(Guid.NewGuid().ToString(), "Template 1", null, null),
            new(Guid.NewGuid().ToString(), "Template 2", null, null)
        };
        
        _mapperMock.Setup(m => m.Map<IEnumerable<TemplateListDto>>(It.IsAny<List<Domain.Entities.Template>>()))
            .Returns(expectedDtos);

        var result = await ListTemplatesHandler.Handle(query, _dbContext, _mapperMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredTemplates_WhenSearchIsProvided()
    {
        var templates = new List<Domain.Entities.Template>
        {
            new() { Name = "Apple" },
            new() { Name = "Banana" },
            new() { Name = "Apricot" }
        };
        _dbContext.Templates.AddRange(templates);
        await _dbContext.SaveChangesAsync();

        var query = new ListTemplatesQuery("Ap");
        
        var expectedDtos = new List<TemplateListDto>
        {
            new(Guid.NewGuid().ToString(), "Apple", null, null),
            new(Guid.NewGuid().ToString(), "Apricot", null, null)
        };
        
        _mapperMock.Setup(m => m.Map<IEnumerable<TemplateListDto>>(It.Is<List<Domain.Entities.Template>>(l => l.Count == 2)))
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
