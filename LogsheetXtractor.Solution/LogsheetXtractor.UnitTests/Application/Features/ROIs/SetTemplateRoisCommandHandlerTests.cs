using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.ROIs;
using LogsheetXtractor.Application.Features.ROIs.DTOs;
using LogsheetXtractor.Application.Features.RoiValidation;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Domain.ValueObjects;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace LogsheetXtractor.UnitTests.Application.Features.ROIs;

public class SetTemplateRoisCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IRoiService> _roiServiceMock = new();
    private readonly Mock<IRoiValidationConditionTreeValidator> _conditionValidatorMock = new();

    public SetTemplateRoisCommandHandlerTests()
    {
        _conditionValidatorMock
            .Setup(x =>
                x.Validate(
                    It.IsAny<ERoiType>(),
                    It.IsAny<LogsheetXtractor.Domain.ValueObjects.RoiValidation.RoiValidationConditionNode>()
                )
            )
            .Returns(Result.Ok());
    }

    [Fact]
    public async Task Handle_ShouldSetRois_WhenTemplateExists()
    {
        var templateId = Guid.NewGuid();
        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = templateId,
            Name = "Test Template",
        };
        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();

        var updateRois = new List<SetRoiDto>
        {
            new(null, "New ROI", ERoiType.Handwritten, new Coordinates(0, 0, 10, 10)),
        };
        var command = new SetTemplateRoisCommand(templateId, updateRois);
        var expectedResult = new List<RoiDto>
        {
            new(
                Guid.NewGuid(),
                "New ROI",
                templateId,
                ERoiType.Handwritten,
                new Coordinates(0, 0, 10, 10),
                DateTime.UtcNow,
                null
            ),
        };

        _roiServiceMock
            .Setup(x =>
                x.SetRoisForTemplateAsync(templateId, updateRois, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(expectedResult);

        var result = await SetTemplateRoisHandler.Handle(
            command,
            _roiServiceMock.Object,
            _dbContext,
            _conditionValidatorMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedResult);
        _roiServiceMock.Verify(
            x => x.SetRoisForTemplateAsync(templateId, updateRois, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldUpsertAndDeleteRois_WhenTemplateExists()
    {
        var templateId = Guid.NewGuid();
        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = templateId,
            Name = "Test Template",
        };
        var templateToDelete = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = Guid.NewGuid(),
            Name = "Template To Delete",
        };
        _dbContext.Templates.Add(template);
        _dbContext.Templates.Add(templateToDelete);
        await _dbContext.SaveChangesAsync();

        var existingRoiId = Guid.NewGuid();
        var updateRois = new List<SetRoiDto>
        {
            new(
                existingRoiId.ToString(),
                "Updated ROI",
                ERoiType.Handwritten,
                new Coordinates(5, 5, 15, 15)
            ),
            new(null, "New ROI", ERoiType.Checkbox, new Coordinates(10, 10, 20, 20)),
        };
        var command = new SetTemplateRoisCommand(templateId, updateRois);
        var expectedResult = new List<RoiDto>
        {
            new(
                existingRoiId,
                "Updated ROI",
                templateId,
                ERoiType.Handwritten,
                new Coordinates(5, 5, 15, 15),
                DateTime.UtcNow,
                null
            ),
            new(
                Guid.NewGuid(),
                "New ROI",
                templateId,
                ERoiType.Checkbox,
                new Coordinates(10, 10, 20, 20),
                DateTime.UtcNow,
                null
            ),
        };

        _roiServiceMock
            .Setup(x =>
                x.SetRoisForTemplateAsync(templateId, updateRois, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(expectedResult);

        var result = await SetTemplateRoisHandler.Handle(
            command,
            _roiServiceMock.Object,
            _dbContext,
            _conditionValidatorMock.Object,
            CancellationToken.None
        );
        await _dbContext.SaveChangesAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedResult);
        _roiServiceMock.Verify(
            x => x.SetRoisForTemplateAsync(templateId, updateRois, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTemplate_NotFound()
    {
        var command = new SetTemplateRoisCommand(Guid.NewGuid(), new List<SetRoiDto>());

        var result = await SetTemplateRoisHandler.Handle(
            command,
            _roiServiceMock.Object,
            _dbContext,
            _conditionValidatorMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Template not found");
        _roiServiceMock.Verify(
            x =>
                x.SetRoisForTemplateAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<SetRoiDto>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenValidationConditionIsInvalid()
    {
        var templateId = Guid.NewGuid();
        _dbContext.Templates.Add(
            new LogsheetXtractor.Domain.Entities.Template { Id = templateId, Name = "Template" }
        );
        await _dbContext.SaveChangesAsync();

        var invalidCondition =
            new LogsheetXtractor.Domain.ValueObjects.RoiValidation.RoiValidationConditionNode
            {
                Type = "group",
                Operator = "AND",
                Children = [],
            };

        var rois = new List<SetRoiDto>
        {
            new(
                null,
                "Value",
                ERoiType.Handwritten,
                new Coordinates(0, 0, 10, 10),
                invalidCondition
            ),
        };
        var command = new SetTemplateRoisCommand(templateId, rois);

        _conditionValidatorMock
            .Setup(x => x.Validate(ERoiType.Handwritten, invalidCondition))
            .Returns(Result.Fail("Invalid condition tree"));

        var result = await SetTemplateRoisHandler.Handle(
            command,
            _roiServiceMock.Object,
            _dbContext,
            _conditionValidatorMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<ValidationError>();
        _roiServiceMock.Verify(
            x =>
                x.SetRoisForTemplateAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<SetRoiDto>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
