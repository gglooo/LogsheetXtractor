using FluentAssertions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Features.ROIs;
using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Domain.ValueObjects;
using WebFormHTR.Infrastructure.Persistence;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.ROIs;

public class SetTemplateRoisCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IRoiService> _roiServiceMock;

    public SetTemplateRoisCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new AppDbContext(options);
        _roiServiceMock = new Mock<IRoiService>();
    }

    [Fact]
    public async Task Handle_ShouldSetRois_WhenTemplateExists()
    {
        var templateId = Guid.NewGuid();
        var template = new Domain.Entities.Template { Id = templateId, Name = "Test Template" };
        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();

        var updateRois = new List<SetRoiDto> { new SetRoiDto(null, "New ROI", ERoiType.Text, new Coordinates { X = 0, Y = 0, Width = 10, Height = 10 }) };
        var command = new SetTemplateRoisCommand(templateId, updateRois);
        var expectedResult = new List<RoiDto> { new RoiDto(Guid.NewGuid(), "New ROI", templateId, ERoiType.Text, new Coordinates { X = 0, Y = 0, Width = 10, Height = 10 }) };

        _roiServiceMock.Setup(x => x.SetRoisForTemplateAsync(templateId, updateRois, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await SetTemplateRoisHandler.Handle(command, _roiServiceMock.Object, _dbContext, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedResult);
        _roiServiceMock.Verify(x => x.SetRoisForTemplateAsync(templateId, updateRois, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTemplate_NotFound()
    {
        var command = new SetTemplateRoisCommand(Guid.NewGuid(), new List<SetRoiDto>());

        var result = await SetTemplateRoisHandler.Handle(command, _roiServiceMock.Object, _dbContext, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Template not found");
        _roiServiceMock.Verify(x => x.SetRoisForTemplateAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<SetRoiDto>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
