using FluentAssertions;
using LogsheetXtractor.Application.Features.RoiValidation;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Domain.ValueObjects.RoiValidation;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using Xunit;

namespace LogsheetXtractor.UnitTests.Application.Features.RoiValidation;

public class GetPredefinedRoiValidationConditionsTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();

    [Fact]
    public async Task Handle_ShouldReturnAllPredefinedConditions_WhenNoRoiTypeFilter()
    {
        await SeedConditions();

        var query = new GetPredefinedRoiValidationConditionsQuery();
        var result = await GetPredefinedRoiValidationConditionsHandler.Handle(
            query,
            _dbContext,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result
            .Value.Select(x => x.Code)
            .Should()
            .Contain(new[] { "year", "month", "barcode-prefix" });
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyMatchingRoiType_WhenRoiTypeFilterProvided()
    {
        await SeedConditions();

        var query = new GetPredefinedRoiValidationConditionsQuery(ERoiType.Number);
        var result = await GetPredefinedRoiValidationConditionsHandler.Handle(
            query,
            _dbContext,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().OnlyContain(x => x.RoiType == ERoiType.Number);
        result.Value.Select(x => x.Code).Should().Contain(new[] { "year", "month" });
    }

    [Fact]
    public async Task Handle_ShouldReturnStableConditionResponseShape()
    {
        await SeedConditions();

        var result = await GetPredefinedRoiValidationConditionsHandler.Handle(
            new GetPredefinedRoiValidationConditionsQuery(ERoiType.Barcode),
            _dbContext,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        var condition = result.Value.Should().ContainSingle().Subject;
        condition.Id.Should().NotBeEmpty();
        condition.Code.Should().Be("barcode-prefix");
        condition.Label.Should().Be("Barcode Prefix");
        condition.RoiType.Should().Be(ERoiType.Barcode);
        condition.Condition.Type.Should().Be("group");
        condition.Condition.Operator.Should().Be("AND");
        condition.Condition.Children.Should().ContainSingle();
        condition.Condition.Children![0].RuleType.Should().Be("text.prefix");
    }

    private async Task SeedConditions()
    {
        var numberCondition = new RoiValidationConditionNode
        {
            Type = "group",
            Operator = "AND",
            Children =
            [
                new RoiValidationConditionNode
                {
                    Type = "rule",
                    RuleType = "common.requiredNonEmpty",
                    Params = System.Text.Json.JsonSerializer.SerializeToElement(new { }),
                },
            ],
        };

        _dbContext.PredefinedRoiValidationConditions.AddRange(
            new PredefinedRoiValidationCondition
            {
                Id = Guid.NewGuid(),
                Code = "year",
                Label = "Year",
                RoiType = ERoiType.Number,
                Condition = numberCondition,
            },
            new PredefinedRoiValidationCondition
            {
                Id = Guid.NewGuid(),
                Code = "month",
                Label = "Month",
                RoiType = ERoiType.Number,
                Condition = numberCondition,
            },
            new PredefinedRoiValidationCondition
            {
                Id = Guid.NewGuid(),
                Code = "barcode-prefix",
                Label = "Barcode Prefix",
                RoiType = ERoiType.Barcode,
                Condition = new RoiValidationConditionNode
                {
                    Type = "group",
                    Operator = "AND",
                    Children =
                    [
                        new RoiValidationConditionNode
                        {
                            Type = "rule",
                            RuleType = "text.prefix",
                            Params = System.Text.Json.JsonSerializer.SerializeToElement(
                                new { prefix = "ABC" }
                            ),
                        },
                    ],
                },
            }
        );

        await _dbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
