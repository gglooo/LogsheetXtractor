using FluentAssertions;
using MapsterMapper;
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

public class CreateRoisCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IMapper> _mapperMock;

    public CreateRoisCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new AppDbContext(options);
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task Handle_ShouldCreateRois_WhenRequestIsValid()
    {
        var templateId = Guid.NewGuid();
        var roiDtos = new List<CreateRoiDto>
        {
            new CreateRoiDto("ROI 1", ERoiType.Text, new Coordinates { X = 0, Y = 0, Width = 10, Height = 10 }),
            new CreateRoiDto("ROI 2", ERoiType.Text, new Coordinates { X = 10, Y = 10, Width = 20, Height = 20 })
        };

        var command = new CreateRoisCommand(templateId, roiDtos);

        _mapperMock.Setup(x => x.Map<List<Roi>>(roiDtos))
            .Returns([
                new Roi
                {
                    VariableName = "ROI 1", Type = ERoiType.Text,
                    Coordinates = new Coordinates { X = 0, Y = 0, Width = 10, Height = 10 }, Template = null!
                },
                new Roi
                {
                    VariableName = "ROI 2", Type = ERoiType.Text,
                    Coordinates = new Coordinates { X = 10, Y = 10, Width = 20, Height = 20 }, Template = null!
                }
            ]);

        await CreateRoisHandler.Handle(command, _dbContext, _mapperMock.Object, CancellationToken.None);

        var rois = await _dbContext.Rois.Where(r => r.TemplateId == templateId).ToListAsync();
        rois.Should().HaveCount(2);
        rois.Should().Contain(r => r.VariableName == "ROI 1");
        rois.Should().Contain(r => r.VariableName == "ROI 2");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
