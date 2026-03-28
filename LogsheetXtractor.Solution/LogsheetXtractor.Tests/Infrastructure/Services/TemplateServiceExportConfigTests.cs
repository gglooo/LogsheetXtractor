using System.Text.Json;
using FluentAssertions;
using LogsheetXtractor.Application.Common.Mappings;
using LogsheetXtractor.Application.Features.Template.CreateTemplate;
using LogsheetXtractor.Application.Features.Residuals;
using LogsheetXtractor.Application.Features.ROIs;
using LogsheetXtractor.Application.Features.Scripting;
using LogsheetXtractor.Application.Features.Template.DTOs;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Domain.ValueObjects;
using LogsheetXtractor.Domain.ValueObjects.RoiValidation;
using LogsheetXtractor.Infrastructure.Mappings;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.Infrastructure.Services;
using Mapster;
using MapsterMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using File = LogsheetXtractor.Domain.Entities.File;

namespace LogsheetXtractor.Tests.Infrastructure.Services;

public sealed class TemplateServiceExportConfigTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly IMapper _mapper;

    public TemplateServiceExportConfigTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;

        using var setupContext = new AppDbContext(_options);
        setupContext.Database.EnsureCreated();

        var mapsterConfig = new TypeAdapterConfig();
        new MappingConfig().Register(mapsterConfig);
        new PythonMappingConfig().Register(mapsterConfig);
        _mapper = new Mapper(mapsterConfig);
    }

    [Fact]
    public async Task ExportTemplateConfigAsync_ShouldNotContainValidationCondition_WhenIncludeRoiValidationsIsFalse()
    {
        var templateId = await SeedTemplateWithValidatedRoiAsync();

        await using var context = new AppDbContext(_options);
        var service = CreateTemplateService(context);

        var result = await service.ExportTemplateConfigAsync(
            templateId,
            includeRoiValidations: false,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        using var json = JsonDocument.Parse(result.Value);
        var roiJson = json.RootElement.GetProperty("content")[0];
        roiJson.TryGetProperty("validation_condition", out _).Should().BeFalse();
    }

    [Fact]
    public async Task ExportTemplateConfigAsync_ShouldExportRoiVariableNames_InsteadOfIds()
    {
        var templateId = await SeedTemplateWithValidatedRoiAsync();

        await using var context = new AppDbContext(_options);
        var service = CreateTemplateService(context);

        var result = await service.ExportTemplateConfigAsync(
            templateId,
            includeRoiValidations: true,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        using var json = JsonDocument.Parse(result.Value);
        var roiJson = json.RootElement.GetProperty("content")[0];
        roiJson.GetProperty("varname").GetString().Should().Be("temperature");
    }

    [Fact]
    public async Task CreateTemplateAsync_ShouldImportOptionalValidationCondition_WhenPresent()
    {
        var fileId = await SeedFileAsync();

        var command = new CreateTemplateCommand
        {
            Name = "Imported template with conditions",
            FileId = fileId,
            ImportedConfig =
                """
                {
                  "content": [
                    {
                      "coords": [10, 20, 40, 60],
                      "type": "Number",
                      "varname": "temperature",
                      "validation_condition": {
                        "type": "group",
                        "operator": "AND",
                        "children": [
                          {
                            "type": "rule",
                            "ruleType": "number.range",
                            "params": { "min": 0, "max": 10 }
                          }
                        ]
                      }
                    }
                  ],
                  "to_ignore": [],
                  "width": 100,
                  "height": 200
                }
                """,
        };

        await using var context = new AppDbContext(_options);
        var service = CreateTemplateService(context);

        var result = await service.CreateTemplateAsync(command, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        await context.SaveChangesAsync();

        var persistedRoi = await context.Rois.AsNoTracking().SingleAsync();
        persistedRoi.ValidationCondition.Should().NotBeNull();
        persistedRoi.ValidationCondition!.Type.Should().Be("group");
        persistedRoi.ValidationCondition.Children.Should().NotBeNull();
        persistedRoi.ValidationCondition.Children!.Single().RuleType.Should().Be("number.range");
    }

    [Fact]
    public async Task CreateTemplateAsync_ShouldImportConfig_WhenValidationConditionIsMissing()
    {
        var fileId = await SeedFileAsync();

        var command = new CreateTemplateCommand
        {
            Name = "Imported template without conditions",
            FileId = fileId,
            ImportedConfig =
                """
                {
                  "content": [
                    {
                      "coords": [10, 20, 40, 60],
                      "type": "Number",
                      "varname": "temperature"
                    }
                  ],
                  "to_ignore": [],
                  "width": 100,
                  "height": 200
                }
                """,
        };

        await using var context = new AppDbContext(_options);
        var service = CreateTemplateService(context);

        var result = await service.CreateTemplateAsync(command, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        await context.SaveChangesAsync();

        var persistedRoi = await context.Rois.AsNoTracking().SingleAsync();
        persistedRoi.ValidationCondition.Should().BeNull();
    }

    private TemplateService CreateTemplateService(AppDbContext context)
    {
        var residualServiceMock = new Mock<IResidualService>();
        var roiServiceMock = new Mock<IRoiService>();
        var scriptEngineMock = new Mock<IHtrScriptEngine>();
        var loggerMock = new Mock<ILogger<TemplateService>>();

        return new TemplateService(
            context,
            _mapper,
            residualServiceMock.Object,
            roiServiceMock.Object,
            scriptEngineMock.Object,
            loggerMock.Object
        );
    }

    private async Task<Guid> SeedTemplateWithValidatedRoiAsync()
    {
        await using var context = new AppDbContext(_options);

        var file = new File
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "template.pdf",
            StoredFileName = "template.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
            SizeBytes = 10,
        };

        var template = new Template
        {
            Id = Guid.NewGuid(),
            Name = "Template A",
            FileId = file.Id,
            File = file,
            Width = 100,
            Height = 200,
        };

        var roi = new Roi
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            Template = template,
            VariableName = "temperature",
            Type = ERoiType.Number,
            Coordinates = new Coordinates(10, 20, 30, 40),
            ValidationCondition = new RoiValidationConditionNode
            {
                Type = "rule",
                RuleType = "number.range",
                Params = JsonSerializer.SerializeToElement(new { min = 0, max = 10 }),
            },
        };

        context.Files.Add(file);
        context.Templates.Add(template);
        context.Rois.Add(roi);
        await context.SaveChangesAsync();

        return template.Id;
    }

    private async Task<Guid> SeedFileAsync()
    {
        await using var context = new AppDbContext(_options);

        var file = new File
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "template.pdf",
            StoredFileName = "template.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
            SizeBytes = 10,
        };

        context.Files.Add(file);
        await context.SaveChangesAsync();

        return file.Id;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
