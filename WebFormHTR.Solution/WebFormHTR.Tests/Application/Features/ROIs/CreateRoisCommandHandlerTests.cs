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
using WebFormHTR.Tests.Common;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.ROIs;

public class CreateRoisCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IMapper> _mapperMock = new();

    [Fact]
    public async Task Handle_ShouldCreateRois_WhenRequestIsValid()
    {
        var template = new Domain.Entities.Template
            { Id = Guid.NewGuid(), Name = "Test Template", FileId = Guid.NewGuid() };

        await _dbContext.Templates.AddAsync(template);
        await _dbContext.SaveChangesAsync();

        var roiDtos = new List<CreateRoiDto>
        {
            new("ROI 1", ERoiType.Handwritten, new Coordinates(0, 0, 10, 10)),
            new("ROI 2", ERoiType.Handwritten, new Coordinates(10, 10, 20, 20))
        };

        var command = new CreateRoisCommand(template.Id, roiDtos);

        _mapperMock.Setup(x => x.Map<List<Roi>>(roiDtos))
            .Returns([
                new Roi
                {
                    VariableName = "ROI 1", Type = ERoiType.Handwritten,
                    Coordinates = new Coordinates(0, 0, 10, 10), Template = null!
                },
                new Roi
                {
                    VariableName = "ROI 2", Type = ERoiType.Handwritten,
                    Coordinates = new Coordinates(10, 10, 20, 20), Template = null!
                }
            ]);

        await CreateRoisHandler.Handle(command, _dbContext, _mapperMock.Object, CancellationToken.None);

        await _dbContext.SaveChangesAsync();

        var rois = await _dbContext.Rois.Where(r => r.TemplateId == template.Id).ToListAsync();
        rois.Should().HaveCount(2);
        rois.Should().Contain(r => r.VariableName == "ROI 1");
        rois.Should().Contain(r => r.VariableName == "ROI 2");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}