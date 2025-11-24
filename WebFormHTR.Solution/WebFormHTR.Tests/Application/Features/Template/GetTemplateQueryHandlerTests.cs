using FluentAssertions;
using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Features.Template;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.Template;

public class GetTemplateQueryHandlerTests
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IMapper> _mapperMock = new();

    [Fact]
    public async Task Handle_ShouldReturnTemplate_WhenTemplateExists()
    {
        var templateId = Guid.NewGuid();
        var template = new WebFormHTR.Domain.Entities.Template { Id = templateId, Name = "Existing Template" };
        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();

        var expectedDto = new TemplateDetailDto(templateId, "Existing Template", null, null, DateTime.UtcNow, DateTime.UtcNow, []);
        _mapperMock.Setup(x => x.Map<TemplateDetailDto>(It.Is<WebFormHTR.Domain.Entities.Template>(t => t.Id == templateId)))
            .Returns(expectedDto);

        var query = new GetTemplateQuery(templateId);

        var result = await GetTemplateHandler.Handle(query, _dbContext, _mapperMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenTemplateDoesNotExist()
    {
        var query = new GetTemplateQuery(Guid.NewGuid());

        var result = await GetTemplateHandler.Handle(query, _dbContext, _mapperMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }
}
