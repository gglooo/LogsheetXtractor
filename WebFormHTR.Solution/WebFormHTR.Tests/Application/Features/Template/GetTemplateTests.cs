using FluentAssertions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Features.Template;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;

namespace WebFormHTR.Tests.Application.Features.Template;

public class GetTemplateTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IMapper> _mapperMock = new();

    [Fact]
    public async Task Handle_ShouldReturnTemplate_WhenFound()
    {
        var template = new Domain.Entities.Template { Name = "Test Template" };
        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();

        var query = new GetTemplateQuery(template.Id);

        var expectedDto = new TemplateDetailDto(template.Id, template.Name, null, null, DateTime.Now, DateTime.Now, []);
        _mapperMock.Setup(m => m.Map<TemplateDetailDto?>(It.IsAny<Domain.Entities.Template>()))
            .Returns(expectedDto);

        var result = await GetTemplateHandler.Handle(query, _dbContext, _mapperMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDto);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenNotFound()
    {
        var query = new GetTemplateQuery(Guid.NewGuid());

        _mapperMock.Setup(m => m.Map<TemplateDetailDto?>(null))
            .Returns((TemplateDetailDto?)null);

        var result = await GetTemplateHandler.Handle(query, _dbContext, _mapperMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}