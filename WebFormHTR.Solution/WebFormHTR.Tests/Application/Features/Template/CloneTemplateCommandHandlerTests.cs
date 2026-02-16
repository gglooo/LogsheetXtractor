using FluentAssertions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Template;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Features.Template.Interfaces;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.Template;

public class CloneTemplateCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<ITemplateService> _templateServiceMock = new();

    [Fact]
    public async Task Handle_ShouldCloneTemplate_WhenRequestIsValid()
    {
        var file = new Domain.Entities.File { Id = Guid.NewGuid(), StoredFileName = "file.pdf" };
        _dbContext.Files.Add(file);
        
        var template = new Domain.Entities.Template { Id = Guid.NewGuid(), Name = "Original Template", FileId = file.Id };
        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();

        var command = new CloneTemplateCommand(template.Id, "Cloned Template", file.Id);

         var expectedDto = new TemplateDetailDto(Guid.NewGuid(), "Cloned Template", 0, 0, null, null, null, null, DateTime.UtcNow, DateTime.UtcNow, [], [], true);

        _templateServiceMock.Setup(x => x.CloneTemplateAsync(command.TemplateId, command.NewTemplateName, command.FileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await CloneTemplateHandler.Handle(command, CancellationToken.None, _templateServiceMock.Object, _dbContext);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);
        _templateServiceMock.Verify(x => x.CloneTemplateAsync(command.TemplateId, command.NewTemplateName, command.FileId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTemplateNotFound()
    {
        var command = new CloneTemplateCommand(Guid.NewGuid(), "Cloned Template", Guid.NewGuid());

        var result = await CloneTemplateHandler.Handle(command, CancellationToken.None, _templateServiceMock.Object, _dbContext);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
        result.Errors.First().Message.Should().Be("Cloned template not found");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenFileNotFound()
    {
        var template = new Domain.Entities.Template { Id = Guid.NewGuid(), Name = "Original Template", FileId = Guid.NewGuid() };
        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();

        var command = new CloneTemplateCommand(template.Id, "Cloned Template", Guid.NewGuid());

        var result = await CloneTemplateHandler.Handle(command, CancellationToken.None, _templateServiceMock.Object, _dbContext);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
        result.Errors.First().Message.Should().Be("Cloned template's file not found");
    }
    
    [Fact]
    public async Task Handle_ShouldReturnFail_WhenServiceThrowsException()
    {
        var file = new Domain.Entities.File { Id = Guid.NewGuid(), StoredFileName = "file.pdf" };
        _dbContext.Files.Add(file);
        
        var template = new Domain.Entities.Template { Id = Guid.NewGuid(), Name = "Original Template", FileId = file.Id };
        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();

        var command = new CloneTemplateCommand(template.Id, "Cloned Template", file.Id);
        var errorMessage = "Service failure";

        _templateServiceMock.Setup(x => x.CloneTemplateAsync(command.TemplateId, command.NewTemplateName, command.FileId, It.IsAny<CancellationToken>()))
             .ThrowsAsync(new Exception(errorMessage));

        var result = await CloneTemplateHandler.Handle(command, CancellationToken.None, _templateServiceMock.Object, _dbContext);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be(errorMessage);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
