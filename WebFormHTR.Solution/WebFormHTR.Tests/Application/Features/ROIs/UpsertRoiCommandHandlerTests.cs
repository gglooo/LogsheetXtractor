using FluentAssertions;
using FluentResults;
using Moq;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.ROIs;
using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Domain.ValueObjects;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.ROIs;

public class UpsertRoiCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IRoiService> _roiServiceMock = new();

    [Fact]
    public async Task Handle_ShouldUpsertRoi_WhenTemplateExists()
    {
        var template = new Domain.Entities.Template { Id = Guid.NewGuid(), Name = "Test Template" };
        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();

        var upsertRoiDto = new UpsertRoiDto(null, "New ROI", ERoiType.Handwritten, new Coordinates(10, 10, 100, 50));
        var command = new UpsertRoiCommand(template.Id, upsertRoiDto);

        var expectedDto = new RoiDto(Guid.NewGuid(), "New ROI", template.Id, ERoiType.Handwritten, upsertRoiDto.Coordinates, DateTime.UtcNow, null);
        
        _roiServiceMock.Setup(x => x.UpsertRoiForTemplateAsync(command.TemplateId, command.Roi, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await UpsertRoiHandler.Handle(command, _dbContext, _roiServiceMock.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);
        _roiServiceMock.Verify(x => x.UpsertRoiForTemplateAsync(command.TemplateId, command.Roi, It.IsAny<CancellationToken>()), Times.Once);
        _dbContext.ChangeTracker.HasChanges().Should().BeFalse(); // Handler saves changes
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTemplateNotFound()
    {
        var upsertRoiDto = new UpsertRoiDto(null, "New ROI", ERoiType.Handwritten, new Coordinates(0, 0, 0, 0));
        var command = new UpsertRoiCommand(Guid.NewGuid(), upsertRoiDto);

        var result = await UpsertRoiHandler.Handle(command, _dbContext, _roiServiceMock.Object, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
        result.Errors.First().Message.Should().Be("Template not found");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenServiceThrowsException()
    {
        var template = new Domain.Entities.Template { Id = Guid.NewGuid(), Name = "Test Template" };
        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();

        var upsertRoiDto = new UpsertRoiDto(null, "New ROI", ERoiType.Handwritten, new Coordinates(0, 0, 0, 0));
        var command = new UpsertRoiCommand(template.Id, upsertRoiDto);
        var errorMessage = "Service failure";

        _roiServiceMock.Setup(x => x.UpsertRoiForTemplateAsync(command.TemplateId, command.Roi, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(errorMessage));

        var result = await UpsertRoiHandler.Handle(command, _dbContext, _roiServiceMock.Object, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be($"Failed to upsert ROI: {errorMessage}");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
