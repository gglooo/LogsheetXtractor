using WebFormHTR.Application.Features.RoiValidation.DTOs;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.RoiValidation;

public sealed class RoiValidationRuleRegistry(IEnumerable<IRoiValidationRuleEvaluator> evaluators) : IRoiValidationRuleRegistry
{
    private readonly IReadOnlyDictionary<string, IRoiValidationRuleEvaluator> _lookup = evaluators
        .ToDictionary(x => x.RuleType, StringComparer.OrdinalIgnoreCase);

    private readonly IReadOnlyList<IRoiValidationRuleEvaluator> _all = evaluators.ToList();

    public bool TryGet(string ruleType, out IRoiValidationRuleEvaluator evaluator)
    {
        return _lookup.TryGetValue(ruleType, out evaluator!);
    }

    public IReadOnlyList<IRoiValidationRuleEvaluator> GetAll()
    {
        return _all;
    }
}

public sealed class RoiValidationRuleCatalogProvider(IRoiValidationRuleRegistry registry) : IRoiValidationRuleCatalogProvider
{
    private readonly RoiValidationRuleCatalogDto _catalog = BuildCatalog(registry.GetAll());

    public RoiValidationRuleCatalogDto GetCatalog()
    {
        return _catalog;
    }

    private static RoiValidationRuleCatalogDto BuildCatalog(IReadOnlyList<IRoiValidationRuleEvaluator> evaluators)
    {
        var byType = evaluators
            .SelectMany(rule => rule.SupportedRoiTypes.Select(type => new { type, rule }))
            .GroupBy(x => x.type)
            .Select(group => new RoiValidationRulesByRoiTypeDto(
                group.Key,
                group
                    .Select(x => new RoiValidationRuleDefinitionDto(
                        x.rule.RuleType,
                        x.rule.Label,
                        x.rule.Description,
                        x.rule.DefaultParams))
                    .OrderBy(x => x.RuleType, StringComparer.Ordinal)
                    .ToList()))
            .OrderBy(x => x.RoiType)
            .ToList();

        return new RoiValidationRuleCatalogDto("v1", byType);
    }
}
