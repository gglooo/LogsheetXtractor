using FluentAssertions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Domain.ValueObjects;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Infrastructure.Services;
using WebFormHTR.Tests.Common;
using Xunit;

namespace WebFormHTR.Tests.Infrastructure.Services;

public class RoiServiceTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IHtrScriptEngine> _scriptEngineMock;
    private readonly RoiService _roiService;

    public RoiServiceTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mapperMock = new Mock<IMapper>();
        _scriptEngineMock = new Mock<IHtrScriptEngine>();

        _roiService = new RoiService(_dbContext, _mapperMock.Object, _scriptEngineMock.Object);
    }

    [Fact]
    public async Task SetRoisForTemplateAsync_ShouldAddNewRois()
    {
        var templateId = Guid.NewGuid();
        var newRoiDto = new SetRoiDto(null, "New ROI", ERoiType.Handwritten,
            new Coordinates { X = 10, Y = 10, Width = 100, Height = 100 });
        var updateRois = new List<SetRoiDto> { newRoiDto };

        _mapperMock.Setup(m => m.Map<Roi>(It.IsAny<SetRoiDto>()))
            .Returns((SetRoiDto dto) => new Roi
            {
                VariableName = dto.VariableName,
                Type = dto.Type,
                Coordinates = dto.Coordinates,
                Template = null!
            });

        _mapperMock.Setup(m => m.Map<IEnumerable<RoiDto>>(It.IsAny<IEnumerable<Roi>>()))
            .Returns((IEnumerable<Roi> rois) =>
                rois.Select(r => new RoiDto(r.Id, r.VariableName, r.TemplateId, r.Type, r.Coordinates)));

        var result =
            (await _roiService.SetRoisForTemplateAsync(templateId, updateRois, CancellationToken.None)).ToList();
        await _dbContext.SaveChangesAsync();

        var savedRois = await _dbContext.Rois.Where(r => r.TemplateId == templateId).ToListAsync();
        savedRois.Should().HaveCount(1);
        savedRois[0].VariableName.Should().Be("New ROI");
        savedRois[0].TemplateId.Should().Be(templateId);

        result.Should().HaveCount(1);
        result.First().VariableName.Should().Be("New ROI");
    }

    [Fact]
    public async Task SetRoisForTemplateAsync_ShouldUpdateExistingRois()
    {
        var templateId = Guid.NewGuid();
        var existingRoiId = Guid.NewGuid();
        var existingRoi = new Roi
        {
            Id = existingRoiId,
            TemplateId = templateId,
            VariableName = "Old Name",
            Type = ERoiType.Handwritten,
            Coordinates = new Coordinates { X = 0, Y = 0, Width = 50, Height = 50 },
            Template = null!
        };
        _dbContext.Rois.Add(existingRoi);
        await _dbContext.SaveChangesAsync();

        var updateDto = new SetRoiDto(existingRoiId, "Updated Name", ERoiType.Handwritten,
            new Coordinates { X = 20, Y = 20, Width = 60, Height = 60 });
        var updateRois = new List<SetRoiDto> { updateDto };

        _mapperMock.Setup(m => m.Map(It.IsAny<SetRoiDto>(), It.IsAny<Roi>()))
            .Callback<SetRoiDto, Roi>((dto, entity) =>
            {
                entity.VariableName = dto.VariableName;
                entity.Type = dto.Type;
                entity.Coordinates = dto.Coordinates;
            });

        _mapperMock.Setup(m => m.Map<IEnumerable<RoiDto>>(It.IsAny<IEnumerable<Roi>>()))
            .Returns((IEnumerable<Roi> rois) =>
                rois.Select(r => new RoiDto(r.Id, r.VariableName, r.TemplateId, r.Type, r.Coordinates)));

        var result =
            (await _roiService.SetRoisForTemplateAsync(templateId, updateRois, CancellationToken.None)).ToList();
        await _dbContext.SaveChangesAsync();

        var savedRoi = await _dbContext.Rois.FirstAsync(r => r.Id == existingRoiId);
        savedRoi.VariableName.Should().Be("Updated Name");
        savedRoi.Coordinates.Width.Should().Be(60);

        result.Should().HaveCount(1);
        result.First().VariableName.Should().Be("Updated Name");
    }

    [Fact]
    public async Task SetRoisForTemplateAsync_ShouldDeleteMissingRois()
    {
        var templateId = Guid.NewGuid();
        var roiToDelete = new Roi
        {
            Id = Guid.NewGuid(), TemplateId = templateId, VariableName = "To Delete", Template = null!,
            Coordinates = new Coordinates()
        };
        var roiToKeep = new Roi
        {
            Id = Guid.NewGuid(), TemplateId = templateId, VariableName = "To Keep", Template = null!,
            Coordinates = new Coordinates()
        };

        _dbContext.Rois.AddRange(roiToDelete, roiToKeep);
        await _dbContext.SaveChangesAsync();

        var updateDto = new SetRoiDto(roiToKeep.Id, "To Keep", ERoiType.Handwritten, new Coordinates());
        var updateRois = new List<SetRoiDto> { updateDto };

        _mapperMock.Setup(m => m.Map(It.IsAny<SetRoiDto>(), It.IsAny<Roi>()))
            .Callback<SetRoiDto, Roi>((dto, entity) => { }); // No changes needed for this test

        _mapperMock.Setup(m => m.Map<IEnumerable<RoiDto>>(It.IsAny<IEnumerable<Roi>>()))
            .Returns((IEnumerable<Roi> rois) =>
                rois.Select(r => new RoiDto(r.Id, r.VariableName, r.TemplateId, r.Type, r.Coordinates)));

        var result =
            (await _roiService.SetRoisForTemplateAsync(templateId, updateRois, CancellationToken.None)).ToList();
        await _dbContext.SaveChangesAsync();

        var allRois = await _dbContext.Rois.Where(r => r.TemplateId == templateId).ToListAsync();
        allRois.Should().HaveCount(1);
        allRois[0].Id.Should().Be(roiToKeep.Id);

        result.Should().HaveCount(1);
        result.First().Id.Should().Be(roiToKeep.Id);
    }

    [Fact]
    public async Task SetRoisForTemplateAsync_ShouldHandleEmptyUpdateList()
    {
        var templateId = Guid.NewGuid();
        var existingRoi = new Roi
        {
            Id = Guid.NewGuid(), TemplateId = templateId, VariableName = "Existing", Template = null!,
            Coordinates = new Coordinates()
        };
        _dbContext.Rois.Add(existingRoi);
        await _dbContext.SaveChangesAsync();

        var updateRois = new List<SetRoiDto>();
        _mapperMock.Setup(m => m.Map<IEnumerable<RoiDto>>(It.IsAny<IEnumerable<Roi>>()))
            .Returns((IEnumerable<Roi> rois) =>
                rois.Select(r => new RoiDto(r.Id, r.VariableName, r.TemplateId, r.Type, r.Coordinates)));

        var result = await _roiService.SetRoisForTemplateAsync(templateId, updateRois, CancellationToken.None);
        await _dbContext.SaveChangesAsync();

        var allRois = await _dbContext.Rois.Where(r => r.TemplateId == templateId).ToListAsync();
        allRois.Should().BeEmpty();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SetRoisForTemplateAsync_ShouldHandleAllNewRois()
    {
        var templateId = Guid.NewGuid();
        var newRois = new List<SetRoiDto>
        {
            new(null, "ROI 1", ERoiType.Handwritten, new Coordinates()),
            new(null, "ROI 2", ERoiType.Checkbox, new Coordinates())
        };

        _mapperMock.Setup(m => m.Map<Roi>(It.IsAny<SetRoiDto>()))
            .Returns((SetRoiDto dto) => new Roi
            {
                VariableName = dto.VariableName,
                Type = dto.Type,
                Coordinates = dto.Coordinates,
                Template = null!
            });

        _mapperMock.Setup(m => m.Map<IEnumerable<RoiDto>>(It.IsAny<IEnumerable<Roi>>()))
            .Returns((IEnumerable<Roi> rois) =>
                rois.Select(r => new RoiDto(r.Id, r.VariableName, r.TemplateId, r.Type, r.Coordinates)));

        var result = await _roiService.SetRoisForTemplateAsync(templateId, newRois, CancellationToken.None);
        await _dbContext.SaveChangesAsync();

        var savedRois = await _dbContext.Rois.Where(r => r.TemplateId == templateId).ToListAsync();
        savedRois.Should().HaveCount(2);
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SetRoisForTemplateAsync_ShouldHandleAllCombinations()
    {
        var templateId = Guid.NewGuid();
        var existingRois = new List<Roi>
        {
            new()
            {
                Id = Guid.NewGuid(), TemplateId = templateId, VariableName = "Existing 1", Template = null!,
                Coordinates = new Coordinates()
            },
            new()
            {
                Id = Guid.NewGuid(), TemplateId = templateId, VariableName = "Existing 2", Template = null!,
                Coordinates = new Coordinates()
            }
        };
        _dbContext.Rois.AddRange(existingRois);
        await _dbContext.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map<Roi>(It.IsAny<SetRoiDto>()))
            .Returns((SetRoiDto dto) => new Roi
            {
                VariableName = dto.VariableName,
                Type = dto.Type,
                Coordinates = dto.Coordinates,
                Template = null!
            });

        var updateRois = new List<SetRoiDto>
        {
            new(existingRois[0].Id, "Updated Existing 1", ERoiType.Handwritten, new Coordinates()),
            new(null, "New ROI", ERoiType.Checkbox, new Coordinates())
        };

        _mapperMock.Setup(m => m.Map(It.IsAny<SetRoiDto>(), It.IsAny<Roi>()))
            .Callback<SetRoiDto, Roi>((dto, entity) =>
            {
                entity.VariableName = dto.VariableName;
                entity.Type = dto.Type;
                entity.Coordinates = dto.Coordinates;
            });
        _mapperMock.Setup(m => m.Map<IEnumerable<RoiDto>>(It.IsAny<IEnumerable<Roi>>()))
            .Returns((IEnumerable<Roi> rois) =>
                rois.Select(r => new RoiDto(r.Id, r.VariableName, r.TemplateId, r.Type, r.Coordinates)));

        var result = await _roiService.SetRoisForTemplateAsync(templateId, updateRois, CancellationToken.None);
        await _dbContext.SaveChangesAsync();

        var allRois = await _dbContext.Rois.Where(r => r.TemplateId == templateId).ToListAsync();
        allRois.Should().HaveCount(2);
        allRois.Any(r => r.VariableName == "Updated Existing 1").Should().BeTrue();
        allRois.Any(r => r.VariableName == "New ROI").Should().BeTrue();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}