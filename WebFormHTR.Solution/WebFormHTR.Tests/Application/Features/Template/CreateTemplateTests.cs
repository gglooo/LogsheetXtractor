using FluentAssertions;
using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Template;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.Template;

public class CreateTemplateTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IMapper> _mapperMock = new();

    [Fact]
    public async Task Handle_ShouldReturnError_WhenParentIdNotFound()
    {
        var command = new CreateTemplateCommand
        {
            Name = "Test Template",
            ParentId = Guid.NewGuid()
        };

        var result = await CreateTemplateHandler.Handle(command, _dbContext, _mapperMock.Object, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
        result.Errors.First().Message.Should().Be("Parent template not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenFileIdNotFound()
    {
        var command = new CreateTemplateCommand
        {
            Name = "Test Template",
            FileId = Guid.NewGuid()
        };

        var result = await CreateTemplateHandler.Handle(command, _dbContext, _mapperMock.Object, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
        result.Errors.First().Message.Should().Be("File not found");
    }

    [Fact]
    public async Task Handle_ShouldCreateTemplate_WhenValidRequest()
    {
        var file = new Domain.Entities.File { OriginalFileName = "test.pdf", StoredFileName = "test.pdf", StoragePath = "path", ContentType = "application/pdf" };
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        var command = new CreateTemplateCommand
        {
            Name = "Test Template",
            FileId = file.Id
        };

        var expectedDto = new TemplateDetailDto(Guid.NewGuid(), command.Name, null, null, DateTime.Now, DateTime.Now, []);
        _mapperMock.Setup(m => m.Map<TemplateDetailDto>(It.IsAny<Domain.Entities.Template>()))
            .Returns(expectedDto);

        var result = await CreateTemplateHandler.Handle(command, _dbContext, _mapperMock.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDto);

        var templateInDb = await _dbContext.Templates.FirstOrDefaultAsync();
        templateInDb.Should().NotBeNull();
        templateInDb!.Name.Should().Be(command.Name);
        templateInDb.FileId.Should().Be(file.Id);
    }

    [Fact]
    public async Task Handle_ShouldCreateTemplateWithParent_WhenParentExists()
    {
        var file = new Domain.Entities.File { OriginalFileName = "test.pdf", StoredFileName = "test.pdf", StoragePath = "path", ContentType = "application/pdf" };
        var parent = new Domain.Entities.Template { Name = "Parent", File = file };
        _dbContext.Templates.Add(parent);
        await _dbContext.SaveChangesAsync();

        var command = new CreateTemplateCommand
        {
            Name = "Child Template",
            ParentId = parent.Id,
            FileId = file.Id
        };

        var expectedDto = new TemplateDetailDto(Guid.NewGuid(), command.Name, null, null, DateTime.Now, DateTime.Now, []);
        _mapperMock.Setup(m => m.Map<TemplateDetailDto>(It.IsAny<Domain.Entities.Template>()))
            .Returns(expectedDto);

        var result = await CreateTemplateHandler.Handle(command, _dbContext, _mapperMock.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var templateInDb = await _dbContext.Templates.FirstOrDefaultAsync(t => t.Name == command.Name);
        templateInDb.Should().NotBeNull();
        templateInDb!.ParentId.Should().Be(parent.Id);
        templateInDb.FileId.Should().Be(file.Id);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}