using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.Features.ROIs;
using LogsheetXtractor.Application.Features.ROIs.DTOs;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Domain.ValueObjects;
using LogsheetXtractor.Infrastructure.Persistence;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace LogsheetXtractor.UnitTests.Application.Features.ROIs;

public class ListRoisForTemplateQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IMapper> _mapperMock;

    public ListRoisForTemplateQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new AppDbContext(options);
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task Handle_ShouldReturnRois_WhenTemplateExists()
    {
        var templateId = Guid.NewGuid();
        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = templateId,
            Name = "Test Template",
        };
        var roi1 = new Roi
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            VariableName = "ROI 1",
            Template = template,
            Coordinates = new Coordinates(0, 0, 0, 0),
        };
        var roi2 = new Roi
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            VariableName = "ROI 2",
            Template = template,
            Coordinates = new Coordinates(0, 0, 0, 0),
        };

        template.Rois.Add(roi1);
        template.Rois.Add(roi2);

        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();

        var query = new ListRoisForTemplateQuery(templateId);
        var expectedDtos = new List<RoiDto>
        {
            new(
                roi1.Id,
                "ROI 1",
                templateId,
                ERoiType.Handwritten,
                new Coordinates(0, 0, 0, 0),
                roi1.CreatedAt,
                roi1.UpdatedAt
            ),
            new(
                roi2.Id,
                "ROI 2",
                templateId,
                ERoiType.Handwritten,
                new Coordinates(0, 0, 0, 0),
                roi2.CreatedAt,
                roi2.UpdatedAt
            ),
        };

        _mapperMock
            .Setup(x => x.Map<IEnumerable<RoiDto>>(It.IsAny<IEnumerable<Roi>>()))
            .Returns(expectedDtos);

        var result = await ListRoisForTemplateHandler.Handle(
            query,
            _dbContext,
            _mapperMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDtos);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTemplate_NotFound()
    {
        var query = new ListRoisForTemplateQuery(Guid.NewGuid());

        var result = await ListRoisForTemplateHandler.Handle(
            query,
            _dbContext,
            _mapperMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Template not found");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
