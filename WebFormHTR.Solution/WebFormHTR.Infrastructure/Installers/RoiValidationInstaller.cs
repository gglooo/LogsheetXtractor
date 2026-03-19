using Microsoft.Extensions.DependencyInjection;
using WebFormHTR.Application.Features.RoiValidation;
using WebFormHTR.Application.Features.RoiValidation.Rules;

namespace WebFormHTR.Infrastructure.Installers;

public static class RoiValidationInstaller
{
    public static IServiceCollection AddRoiValidation(this IServiceCollection services)
    {
        services.AddScoped<IRoiValidationRuleEvaluator, RequiredNonEmptyRuleEvaluator>();
        services.AddScoped<IRoiValidationRuleEvaluator, NumberRangeRuleEvaluator>();
        services.AddScoped<IRoiValidationRuleEvaluator, NumberIntegerOnlyRuleEvaluator>();
        services.AddScoped<IRoiValidationRuleEvaluator, NumberDecimalScaleMaxRuleEvaluator>();
        services.AddScoped<IRoiValidationRuleEvaluator, NumberNotInSetRuleEvaluator>();
        services.AddScoped<IRoiValidationRuleEvaluator, TextMinLengthRuleEvaluator>();
        services.AddScoped<IRoiValidationRuleEvaluator, TextMaxLengthRuleEvaluator>();
        services.AddScoped<IRoiValidationRuleEvaluator, TextRegexRuleEvaluator>();
        services.AddScoped<IRoiValidationRuleEvaluator, TextNotRegexRuleEvaluator>();
        services.AddScoped<IRoiValidationRuleEvaluator, TextAllowedValuesRuleEvaluator>();
        services.AddScoped<IRoiValidationRuleEvaluator, TextPrefixRuleEvaluator>();
        services.AddScoped<IRoiValidationRuleEvaluator, TextSuffixRuleEvaluator>();

        services.AddScoped<IRoiValidationRuleRegistry, RoiValidationRuleRegistry>();
        services.AddScoped<IRoiValidationRuleCatalogProvider, RoiValidationRuleCatalogProvider>();
        services.AddScoped<IRoiValidationConditionTreeValidator, RoiValidationConditionTreeValidator>();
        services.AddScoped<IRoiValidationConditionNodeEvaluator, RoiValidationGroupNodeEvaluator>();
        services.AddScoped<IRoiValidationConditionNodeEvaluator, RoiValidationRuleNodeEvaluator>();
        services.AddScoped<IRoiValidationConditionEvaluator, RoiValidationConditionEvaluator>();

        return services;
    }
}
