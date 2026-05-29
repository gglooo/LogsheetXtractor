using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Template;
using LogsheetXtractor.Application.Features.Template.CreateTemplate;
using LogsheetXtractor.Application.Features.Template.DTOs;
using LogsheetXtractor.Application.Features.Template.Interfaces;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace LogsheetXtractor.UnitTests.Application.Features.Template;

public class CreateTemplateTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ITemplateService> _templateServiceMock = new();

    [Fact]
    public async Task Handle_ShouldReturnError_WhenParentIdNotFound()
    {
        var command = new CreateTemplateCommand
        {
            Name = "Test Template",
            ParentId = Guid.NewGuid(),
        };

        var result = await CreateTemplateHandler.Handle(
            command,
            _dbContext,
            _templateServiceMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
        result.Errors.First().Message.Should().Be("Parent template not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenFileIdNotFound()
    {
        var command = new CreateTemplateCommand { Name = "Test Template", FileId = Guid.NewGuid() };

        var result = await CreateTemplateHandler.Handle(
            command,
            _dbContext,
            _templateServiceMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
        result.Errors.First().Message.Should().Be("File not found");
    }

    [Fact]
    public async Task Handle_ShouldCreateTemplate_WhenValidRequest()
    {
        var file = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "test.pdf",
            StoredFileName = "test.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        var command = new CreateTemplateCommand { Name = "Test Template", FileId = file.Id };

        var expectedDto = new TemplateDetailDto(
            Guid.NewGuid(),
            command.Name,
            0,
            0,
            null,
            null,
            null,
            null,
            DateTime.Now,
            DateTime.Now,
            [],
            [],
            true
        );
        _templateServiceMock
            .Setup(s => s.CreateTemplateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await CreateTemplateHandler.Handle(
            command,
            _dbContext,
            _templateServiceMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDto);

        _templateServiceMock.Verify(
            s => s.CreateTemplateAsync(command, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateTemplateWithParent_WhenParentExists()
    {
        var file = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "test.pdf",
            StoredFileName = "test.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };
        var parent = new LogsheetXtractor.Domain.Entities.Template { Name = "Parent", File = file };
        _dbContext.Templates.Add(parent);
        await _dbContext.SaveChangesAsync();

        var command = new CreateTemplateCommand
        {
            Name = "Child Template",
            ParentId = parent.Id,
            FileId = file.Id,
        };

        var expectedDto = new TemplateDetailDto(
            Guid.NewGuid(),
            command.Name,
            0,
            0,
            null,
            null,
            null,
            null,
            DateTime.Now,
            DateTime.Now,
            [],
            [],
            true
        );
        _templateServiceMock
            .Setup(s => s.CreateTemplateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await CreateTemplateHandler.Handle(
            command,
            _dbContext,
            _templateServiceMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();

        _templateServiceMock.Verify(
            s => s.CreateTemplateAsync(command, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
