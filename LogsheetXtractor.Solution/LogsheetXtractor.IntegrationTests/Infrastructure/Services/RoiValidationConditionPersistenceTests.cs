using System.Text.Json;
using FluentAssertions;
using LogsheetXtractor.Application.Common.Mappings;
using LogsheetXtractor.Application.Features.ROIs.DTOs;
using LogsheetXtractor.Application.Features.Scripting;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Domain.ValueObjects;
using LogsheetXtractor.Domain.ValueObjects.RoiValidation;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.Infrastructure.Services;
using Mapster;
using MapsterMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using File = LogsheetXtractor.Domain.Entities.File;

namespace LogsheetXtractor.IntegrationTests.Infrastructure.Services;

public class RoiValidationConditionPersistenceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly IMapper _mapper;

    public RoiValidationConditionPersistenceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;

        using var setupContext = new AppDbContext(_options);
        setupContext.Database.EnsureCreated();

        var mapsterConfig = new TypeAdapterConfig();
        new MappingConfig().Register(mapsterConfig);
        _mapper = new Mapper(mapsterConfig);
    }

    [Fact]
    public async Task SetRoisForTemplateAsync_ShouldPersistValidationCondition_WhenInitiallyNull()
    {
        var (templateId, roiId) = await SeedTemplateWithSingleRoiAsync(
            initialValidationCondition: null
        );

        var firstCondition = BuildGroupCondition("number.range", new { min = 0, max = 10 });

        await using (var dbContext = new AppDbContext(_options))
        {
            var service = new RoiService(dbContext, _mapper, Mock.Of<IHtrScriptEngine>());

            var result = await service.SetRoisForTemplateAsync(
                templateId,
                [
                    new SetRoiDto(
                        roiId.ToString(),
                        "ROI-1",
                        ERoiType.Number,
                        new Coordinates(10, 20, 30, 40),
                        firstCondition
                    ),
                ],
                CancellationToken.None
            );

            result.IsSuccess.Should().BeTrue();
            await dbContext.SaveChangesAsync();
        }

        await using var verifyContext = new AppDbContext(_options);
        var persisted = await verifyContext.Rois.AsNoTracking().SingleAsync(r => r.Id == roiId);

        persisted.ValidationCondition.Should().NotBeNull();
        persisted.ValidationCondition!.Children.Should().HaveCount(1);
        persisted.ValidationCondition.Children![0].RuleType.Should().Be("number.range");
    }

    [Fact]
    public async Task SetRoisForTemplateAsync_ShouldPersistUpdatedValidationCondition_WhenAlreadySet()
    {
        var initialCondition = BuildGroupCondition("number.range", new { min = 0, max = 10 });
        var updatedCondition = BuildGroupCondition("number.integerOnly", new { });

        var (templateId, roiId) = await SeedTemplateWithSingleRoiAsync(initialCondition);

        await using (var dbContext = new AppDbContext(_options))
        {
            var service = new RoiService(dbContext, _mapper, Mock.Of<IHtrScriptEngine>());

            var firstUpdateResult = await service.SetRoisForTemplateAsync(
                templateId,
                [
                    new SetRoiDto(
                        roiId.ToString(),
                        "ROI-1",
                        ERoiType.Number,
                        new Coordinates(10, 20, 30, 40),
                        initialCondition
                    ),
                ],
                CancellationToken.None
            );

            firstUpdateResult.IsSuccess.Should().BeTrue();
            await dbContext.SaveChangesAsync();

            var secondUpdateResult = await service.SetRoisForTemplateAsync(
                templateId,
                [
                    new SetRoiDto(
                        roiId.ToString(),
                        "ROI-1",
                        ERoiType.Number,
                        new Coordinates(10, 20, 30, 40),
                        updatedCondition
                    ),
                ],
                CancellationToken.None
            );

            secondUpdateResult.IsSuccess.Should().BeTrue();
            secondUpdateResult
                .Value.Single()
                .ValidationCondition!.Children![0]
                .RuleType.Should()
                .Be("number.integerOnly");
            await dbContext.SaveChangesAsync();
        }

        await using var verifyContext = new AppDbContext(_options);
        var persisted = await verifyContext.Rois.AsNoTracking().SingleAsync(r => r.Id == roiId);

        persisted.ValidationCondition.Should().NotBeNull();
        persisted.ValidationCondition!.Children.Should().HaveCount(1);
        persisted.ValidationCondition.Children![0].RuleType.Should().Be("number.integerOnly");
        persisted.ValidationCondition.Children![0].RuleType.Should().NotBe("number.range");
    }

    [Fact]
    public async Task SetRoisForTemplateAsync_ShouldPersistNestedValidationGroupsAndRuleParameters()
    {
        var (templateId, roiId) = await SeedTemplateWithSingleRoiAsync(
            initialValidationCondition: null
        );

        var nestedCondition = new RoiValidationConditionNode
        {
            Type = "group",
            Operator = "OR",
            Children =
            [
                new RoiValidationConditionNode
                {
                    Type = "group",
                    Operator = "AND",
                    Children =
                    [
                        new RoiValidationConditionNode
                        {
                            Type = "rule",
                            RuleType = "number.range",
                            Params = JsonSerializer.SerializeToElement(new { min = 10, max = 20 }),
                        },
                    ],
                },
                new RoiValidationConditionNode
                {
                    Type = "rule",
                    RuleType = "number.notInSet",
                    Params = JsonSerializer.SerializeToElement(new { values = new[] { 13, 17 } }),
                },
            ],
        };

        await using (var dbContext = new AppDbContext(_options))
        {
            var service = new RoiService(dbContext, _mapper, Mock.Of<IHtrScriptEngine>());

            var result = await service.SetRoisForTemplateAsync(
                templateId,
                [
                    new SetRoiDto(
                        roiId.ToString(),
                        "ROI-1",
                        ERoiType.Number,
                        new Coordinates(10, 20, 30, 40),
                        nestedCondition
                    ),
                ],
                CancellationToken.None
            );

            result.IsSuccess.Should().BeTrue();
            await dbContext.SaveChangesAsync();
        }

        await using var verifyContext = new AppDbContext(_options);
        var persisted = await verifyContext.Rois.AsNoTracking().SingleAsync(r => r.Id == roiId);

        persisted.ValidationCondition.Should().NotBeNull();
        persisted.ValidationCondition!.Operator.Should().Be("OR");
        persisted.ValidationCondition.Children.Should().HaveCount(2);
        persisted.ValidationCondition.Children![0].Children![0].RuleType.Should().Be("number.range");
        persisted
            .ValidationCondition.Children![0]
            .Children![0]
            .Params!.Value.GetProperty("max")
            .GetInt32()
            .Should()
            .Be(20);
        persisted.ValidationCondition.Children![1].RuleType.Should().Be("number.notInSet");
    }

    private async Task<(Guid templateId, Guid roiId)> SeedTemplateWithSingleRoiAsync(
        RoiValidationConditionNode? initialValidationCondition
    )
    {
        await using var context = new AppDbContext(_options);

        var file = new File
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "template.pdf",
            StoredFileName = "template.pdf",
            StoragePath = "./app_data",
            ContentType = "application/pdf",
            SizeBytes = 10,
        };

        var template = new Template
        {
            Id = Guid.NewGuid(),
            Name = $"Template-{Guid.NewGuid():N}",
            Width = 1000,
            Height = 1400,
            FileId = file.Id,
            File = file,
        };

        var roi = new Roi
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            Template = template,
            VariableName = "ROI-1",
            Type = ERoiType.Number,
            Coordinates = new Coordinates(10, 20, 30, 40),
            ValidationCondition = initialValidationCondition,
        };

        context.Files.Add(file);
        context.Templates.Add(template);
        context.Rois.Add(roi);

        await context.SaveChangesAsync();

        return (template.Id, roi.Id);
    }

    private static RoiValidationConditionNode BuildGroupCondition(
        string ruleType,
        object parameters
    )
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

    public void Dispose()
    {
        _connection.Dispose();
    }
}
