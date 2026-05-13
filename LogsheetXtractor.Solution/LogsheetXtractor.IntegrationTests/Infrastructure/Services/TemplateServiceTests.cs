using FluentAssertions;
using LogsheetXtractor.Application.Features.Residuals;
using LogsheetXtractor.Application.Features.ROIs;
using LogsheetXtractor.Application.Features.Scripting;
using LogsheetXtractor.Application.Features.Scripting.DTOs;
using LogsheetXtractor.Application.Features.Template;
using LogsheetXtractor.Application.Features.Template.DTOs;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Domain.ValueObjects.RoiValidation;
using LogsheetXtractor.Domain.ValueObjects;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.Infrastructure.Services;
using LogsheetXtractor.Infrastructure.Services.Scripting.DTOs;
using LogsheetXtractor.IntegrationTests.Common;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace LogsheetXtractor.IntegrationTests.Infrastructure.Services;

public class TemplateServiceTests
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IHtrScriptEngine> _scriptEngineMock;
    private readonly Mock<ILogger<TemplateService>> _loggerMock;
    private readonly TemplateService _templateService;

    public TemplateServiceTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mapperMock = new Mock<IMapper>();
        var residualServiceMock = new ResidualService(_dbContext, _mapperMock.Object);
        var roiServiceMock = new RoiService(
            _dbContext,
            _mapperMock.Object,
            new Mock<IHtrScriptEngine>().Object
        );
        _scriptEngineMock = new Mock<IHtrScriptEngine>();
        _loggerMock = new Mock<ILogger<TemplateService>>();
        _templateService = new TemplateService(
            _dbContext,
            _mapperMock.Object,
            residualServiceMock,
            roiServiceMock,
            _scriptEngineMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CloneTemplateAsync_ShouldCloneTemplate_WhenParentExists()
    {
        var parentFile = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "parent.pdf",
            StoredFileName = "parent.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };
        _dbContext.Files.Add(parentFile);
        await _dbContext.SaveChangesAsync();

        var parentId = Guid.NewGuid();
        var parentTemplate = new Template
        {
            Id = parentId,
            Name = "Parent Template",
            FileId = parentFile.Id,
        };
        _dbContext.Templates.Add(parentTemplate);
        await _dbContext.SaveChangesAsync();

        var newFile = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "new.pdf",
            StoredFileName = "new.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };
        _dbContext.Files.Add(newFile);
        await _dbContext.SaveChangesAsync();

        var newTemplateName = "Cloned Template";
        var fileId = newFile.Id;

        var expectedDto = new TemplateDetailDto(
            Guid.NewGuid(),
            newTemplateName,
            0,
            0,
            null,
            null,
            null,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow,
            [],
            [],
            true
        );
        _mapperMock.Setup(x => x.Map<TemplateDetailDto>(It.IsAny<Template>())).Returns(expectedDto);
        _scriptEngineMock
            .Setup(x =>
                x.GetPdfDimensionsAsync(
                    It.IsAny<LogsheetXtractor.Domain.Entities.File>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new PdfDimensionsDto { Width = 100, Height = 200 });

        var result = await _templateService.CloneTemplateAsync(
            parentId,
            newTemplateName,
            fileId,
            null,
            CancellationToken.None
        );
        await _dbContext.SaveChangesAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);
        _dbContext
            .Templates.Should()
            .Contain(t =>
                t.Name == newTemplateName && t.ParentId == parentId && t.FileId == fileId
            );
    }

    [Fact]
    public async Task CloneTemplateAsync_ShouldCloneRoisAndResiduals_WhenParentExists()
    {
        var parentFile = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "parent.pdf",
            StoredFileName = "parent.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };
        _dbContext.Files.Add(parentFile);
        await _dbContext.SaveChangesAsync();

        var parentId = Guid.NewGuid();
        var parentTemplate = new Template
        {
            Id = parentId,
            Name = "Parent Template",
            FileId = parentFile.Id,
        };
        _dbContext.Templates.Add(parentTemplate);
        await _dbContext.SaveChangesAsync();

        var roi = new Roi
        {
            TemplateId = parentId,
            VariableName = "ROI 1",
            Coordinates = new Coordinates(4, 5, 3, 2),
            Template = parentTemplate,
        };
        var residual = new Residual
        {
            TemplateId = parentId,
            Template = parentTemplate,
            Content = "Residual 1",
            Coordinates = new Coordinates(4, 5, 3, 2),
        };
        _dbContext.Rois.Add(roi);
        _dbContext.Residuals.Add(residual);
        await _dbContext.SaveChangesAsync();

        var newFile = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "new.pdf",
            StoredFileName = "new.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };
        _dbContext.Files.Add(newFile);
        await _dbContext.SaveChangesAsync();

        var newTemplateName = "Cloned Template";
        var fileId = newFile.Id;

        _mapperMock
            .Setup(x => x.Map<TemplateDetailDto>(It.IsAny<Template>()))
            .Returns(
                new TemplateDetailDto(
                    Guid.NewGuid(),
                    newTemplateName,
                    0,
                    0,
                    null,
                    null,
                    null,
                    null,
                    DateTime.UtcNow,
                    DateTime.UtcNow,
                    [],
                    [],
                    true
                )
            );

        _mapperMock
            .Setup(x => x.Map<Residual>(It.IsAny<Residual>()))
            .Returns(
                (Residual r) =>
                    new Residual
                    {
                        Id = r.Id,
                        Content = r.Content,
                        Coordinates = r.Coordinates,
                    }
            );

        _scriptEngineMock
            .Setup(x =>
                x.GetPdfDimensionsAsync(
                    It.IsAny<LogsheetXtractor.Domain.Entities.File>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new PdfDimensionsDto { Width = 100, Height = 200 });

        _mapperMock
            .Setup(x => x.Map<Roi>(It.IsAny<Roi>()))
            .Returns(
                (Roi r) =>
                    new Roi
                    {
                        Id = r.Id,
                        VariableName = r.VariableName,
                        Coordinates = r.Coordinates,
                        Type = r.Type,
                    }
            );

        var result = await _templateService.CloneTemplateAsync(
            parentId,
            newTemplateName,
            fileId,
            null,
            CancellationToken.None
        );
        await _dbContext.SaveChangesAsync();

        result.IsSuccess.Should().BeTrue();

        var clonedTemplate = await _dbContext.Templates.FirstOrDefaultAsync(t =>
            t.Name == newTemplateName && t.ParentId == parentId
        );

        clonedTemplate.Should().NotBeNull();

        var clonedRois = await _dbContext
            .Rois.Where(r => r.TemplateId == clonedTemplate!.Id)
            .ToListAsync();
        clonedRois.Should().HaveCount(1);
        clonedRois[0].VariableName.Should().BeEquivalentTo(roi.VariableName);
        clonedRois[0].Coordinates.Should().BeEquivalentTo(roi.Coordinates);
        clonedRois[0].Id.Should().NotBe(roi.Id);

        var clonedResiduals = await _dbContext
            .Residuals.Where(r => r.TemplateId == clonedTemplate!.Id)
            .ToListAsync();
        clonedResiduals.Should().HaveCount(1);
        clonedResiduals[0].Content.Should().Be(residual.Content);
    }

    [Fact]
    public async Task CloneTemplateAsync_ShouldCloneBacksideRoisResidualsAndValidationConfig_WhenBacksideProvided()
    {
        var frontFile = await AddFileAsync("front.pdf");
        var backFile = await AddFileAsync("back.pdf");
        var cloneFrontFile = await AddFileAsync("clone-front.pdf");
        var cloneBackFile = await AddFileAsync("clone-back.pdf");

        var parentTemplate = new Template
        {
            Id = Guid.NewGuid(),
            Name = "Parent Front",
            FileId = frontFile.Id,
        };
        var parentBackside = new Template
        {
            Id = Guid.NewGuid(),
            Name = "Parent Back",
            FileId = backFile.Id,
        };
        parentTemplate.SetBacksideTemplate(parentBackside);

        var frontCondition = BuildGroupCondition("number.range", new { min = 1, max = 10 });
        var backCondition = BuildGroupCondition("text.prefix", new { prefix = "ABC" });

        _dbContext.Templates.AddRange(parentTemplate, parentBackside);
        _dbContext.Rois.AddRange(
            new Roi
            {
                TemplateId = parentTemplate.Id,
                Template = parentTemplate,
                VariableName = "front_value",
                Type = ERoiType.Number,
                Coordinates = new Coordinates(1, 2, 30, 40),
                ValidationCondition = frontCondition,
            },
            new Roi
            {
                TemplateId = parentBackside.Id,
                Template = parentBackside,
                VariableName = "back_value",
                Type = ERoiType.Handwritten,
                Coordinates = new Coordinates(5, 6, 70, 80),
                ValidationCondition = backCondition,
            }
        );
        _dbContext.Residuals.AddRange(
            new Residual
            {
                TemplateId = parentTemplate.Id,
                Template = parentTemplate,
                Content = "front residual",
                Coordinates = new Coordinates(2, 3, 4, 5),
            },
            new Residual
            {
                TemplateId = parentBackside.Id,
                Template = parentBackside,
                Content = "back residual",
                Coordinates = new Coordinates(6, 7, 8, 9),
            }
        );
        await _dbContext.SaveChangesAsync();

        _mapperMock
            .Setup(x => x.Map<TemplateDetailDto>(It.IsAny<Template>()))
            .Returns(
                (Template t) =>
                    new TemplateDetailDto(
                        t.Id,
                        t.Name,
                        t.Width ?? 0,
                        t.Height ?? 0,
                        null,
                        null,
                        null,
                        null,
                        DateTime.UtcNow,
                        DateTime.UtcNow,
                        [],
                        [],
                        true
                    )
            );
        _mapperMock.Setup(x => x.Map<Roi>(It.IsAny<Roi>())).Returns((Roi r) => CopyRoi(r));
        _mapperMock
            .Setup(x => x.Map<Residual>(It.IsAny<Residual>()))
            .Returns((Residual r) => CopyResidual(r));
        _scriptEngineMock
            .Setup(x =>
                x.GetPdfDimensionsAsync(
                    It.IsAny<LogsheetXtractor.Domain.Entities.File>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new PdfDimensionsDto { Width = 100, Height = 200 });

        var result = await _templateService.CloneTemplateAsync(
            parentTemplate.Id,
            "Clone Front",
            cloneFrontFile.Id,
            new CloneTemplateBacksideCommand(cloneBackFile.Id),
            CancellationToken.None
        );
        await _dbContext.SaveChangesAsync();

        result.IsSuccess.Should().BeTrue();

        var clonedFront = await _dbContext
            .Templates.Include(t => t.BacksideTemplate)
            .SingleAsync(t => t.Name == "Clone Front");
        var clonedBack = clonedFront.BacksideTemplate;
        clonedBack.Should().NotBeNull();
        clonedBack!.ParentId.Should().Be(parentBackside.Id);

        var clonedFrontRoi = await _dbContext.Rois.SingleAsync(r =>
            r.TemplateId == clonedFront.Id && r.VariableName == "front_value"
        );
        var clonedBackRoi = await _dbContext.Rois.SingleAsync(r =>
            r.TemplateId == clonedBack.Id && r.VariableName == "back_value"
        );

        clonedFrontRoi.ValidationCondition!.Children![0].RuleType.Should().Be("number.range");
        clonedBackRoi.ValidationCondition!.Children![0].RuleType.Should().Be("text.prefix");

        (await _dbContext.Residuals.CountAsync(r => r.TemplateId == clonedFront.Id))
            .Should()
            .Be(1);
        (await _dbContext.Residuals.CountAsync(r => r.TemplateId == clonedBack.Id))
            .Should()
            .Be(1);
    }

    [Fact]
    public async Task ExportTemplateConfigAsync_ShouldIncludeRoiValidation_WhenIncludeRoiValidationsIsTrue()
    {
        var template = await CreateTemplateWithFileAsync();

        _mapperMock
            .Setup(x => x.Map<PythonTemplateConfig>(It.IsAny<Template>()))
            .Returns(
                new PythonTemplateConfig
                {
                    Width = 100,
                    Height = 200,
                    Rois =
                    [
                        new PythonRoiDto
                        {
                            Coords = [0, 0, 10, 10],
                            Type = ERoiType.Number.ToString(),
                            VarName = "temperature",
                            ValidationCondition = new RoiValidationConditionNode
                            {
                                Type = "rule",
                                RuleType = "number.range",
                            },
                        },
                    ],
                }
            );

        var result = await _templateService.ExportTemplateConfigAsync(
            template.Id,
            includeRoiValidations: true,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        using var json = JsonDocument.Parse(result.Value);
        var roi = json.RootElement.GetProperty("content")[0];
        roi.TryGetProperty("validation_condition", out var validationProperty).Should().BeTrue();
        validationProperty.GetProperty("Type").GetString().Should().Be("rule");
    }

    [Fact]
    public async Task ExportTemplateConfigAsync_ShouldOmitRoiValidation_WhenIncludeRoiValidationsIsFalse()
    {
        var template = await CreateTemplateWithFileAsync();

        _mapperMock
            .Setup(x => x.Map<PythonTemplateConfig>(It.IsAny<Template>()))
            .Returns(
                new PythonTemplateConfig
                {
                    Width = 100,
                    Height = 200,
                    Rois =
                    [
                        new PythonRoiDto
                        {
                            Coords = [0, 0, 10, 10],
                            Type = ERoiType.Number.ToString(),
                            VarName = "temperature",
                            ValidationCondition = new RoiValidationConditionNode
                            {
                                Type = "rule",
                                RuleType = "number.range",
                            },
                        },
                    ],
                }
            );

        var result = await _templateService.ExportTemplateConfigAsync(
            template.Id,
            includeRoiValidations: false,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        using var json = JsonDocument.Parse(result.Value);
        var roi = json.RootElement.GetProperty("content")[0];
        roi.TryGetProperty("validation_condition", out _).Should().BeFalse();
    }

    private async Task<Template> CreateTemplateWithFileAsync()
    {
        var file = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "template.pdf",
            StoredFileName = "template.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        var template = new Template
        {
            Name = "Template for export",
            FileId = file.Id,
        };
        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();

        return template;
    }

    private async Task<LogsheetXtractor.Domain.Entities.File> AddFileAsync(string fileName)
    {
        var file = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = fileName,
            StoredFileName = fileName,
            StoragePath = "path",
            ContentType = "application/pdf",
        };
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();
        return file;
    }

    private static Roi CopyRoi(Roi roi)
    {
        return new Roi
        {
            Id = roi.Id,
            VariableName = roi.VariableName,
            Coordinates = roi.Coordinates,
            Type = roi.Type,
            ValidationCondition = roi.ValidationCondition,
        };
    }

    private static Residual CopyResidual(Residual residual)
    {
        return new Residual
        {
            Id = residual.Id,
            Content = residual.Content,
            Coordinates = residual.Coordinates,
        };
    }

    private static RoiValidationConditionNode BuildGroupCondition(string ruleType, object parameters)
    {
        return new RoiValidationConditionNode
        {
            Type = "group",
            Operator = "AND",
            Children =
            [
                new RoiValidationConditionNode
                {
                    Type = "rule",
                    RuleType = ruleType,
                    Params = JsonSerializer.SerializeToElement(parameters),
                },
            ],
        };
    }
}
