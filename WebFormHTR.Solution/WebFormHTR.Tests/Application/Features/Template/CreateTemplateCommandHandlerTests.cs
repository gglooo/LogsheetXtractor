using FluentAssertions;
using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Features.Template;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Infrastructure.Persistence;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.Template;

public class CreateTemplateCommandHandlerTests
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IMapper> _mapperMock;

    public CreateTemplateCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new AppDbContext(options);
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task Handle_ShouldCreateTemplate_WhenRequestIsValid()
    {
        var command = new CreateTemplateCommand { Name = "New Template" };
        var expectedDto = new TemplateDetailDto(Guid.NewGuid(), "New Template", null, null, DateTime.UtcNow, DateTime.UtcNow);

        _mapperMock.Setup(x => x.Map<TemplateDetailDto>(It.IsAny<WebFormHTR.Domain.Entities.Template>()))
            .Returns(expectedDto);

        var result = await CreateTemplateHandler.Handle(command, _dbContext, _mapperMock.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);
        _dbContext.Templates.Should().ContainSingle(t => t.Name == "New Template");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenParentNotFound()
    {
        var command = new CreateTemplateCommand { Name = "Child Template", ParentId = Guid.NewGuid() };

        var result = await CreateTemplateHandler.Handle(command, _dbContext, _mapperMock.Object, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Parent template not found");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenFileNotFound()
    {
        var command = new CreateTemplateCommand { Name = "Template with File", FileId = Guid.NewGuid() };

        var result = await CreateTemplateHandler.Handle(command, _dbContext, _mapperMock.Object, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "File not found");
    }
}
