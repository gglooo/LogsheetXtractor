using FluentAssertions;
using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Application.Features.Template;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Features.Template.Interfaces;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.Template;

public class CreateTemplateCommandHandlerTests
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ITemplateService> _templateServiceMock = new();

    [Fact]
    public async Task Handle_ShouldCreateTemplate_WhenRequestIsValid()
    {
        var file = new Domain.Entities.File { Id = Guid.NewGuid(), StoredFileName = "file.pdf" };
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        var command = new CreateTemplateCommand { Name = "New Template", FileId = file.Id };

        var expectedDto = new TemplateDetailDto(Guid.NewGuid(), "New Template", 0f, 0f, null,
            new FileDto(file.Id, file.StoredFileName, file.ContentType, file.SizeBytes, file.CreatedAt),
            DateTime.UtcNow,
            DateTime.UtcNow, [], []);

        _templateServiceMock.Setup(x => x.CreateTemplateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result =
            await CreateTemplateHandler.Handle(command, _dbContext, _mapperMock.Object, _templateServiceMock.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);
        _templateServiceMock.Verify(x => x.CreateTemplateAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenParentNotFound()
    {
        var command = new CreateTemplateCommand { Name = "Child Template", ParentId = Guid.NewGuid() };

        var result =
            await CreateTemplateHandler.Handle(command, _dbContext, _mapperMock.Object, _templateServiceMock.Object, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Parent template not found");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenFileNotFound()
    {
        var command = new CreateTemplateCommand { Name = "Template with File", FileId = Guid.NewGuid() };

        var result =
            await CreateTemplateHandler.Handle(command, _dbContext, _mapperMock.Object, _templateServiceMock.Object, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "File not found");
    }
}