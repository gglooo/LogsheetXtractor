using System.Text.Json;
using FluentResults;
using LogsheetXtractor.Application.Features.RoiValidation.DTOs;
using LogsheetXtractor.Domain.Enums;

namespace LogsheetXtractor.Application.Features.RoiValidation;

public sealed record RoiValidationRuleEvaluationContext(ERoiType RoiType, string? Value);

internal sealed class RoiValidationWarningError : Error
{
    public RoiValidationWarningError(string code, string message, string path)
        : base(message)
    {
        Warning = new RoiValidationWarningDto(code, message, path);
        Metadata["code"] = code;
        Metadata["path"] = path;
    }

    public RoiValidationWarningDto Warning { get; }
}

internal static class RoiValidationResultExtensions
{
    public static IReadOnlyList<RoiValidationWarningDto> ToWarnings(this Result result)
    {
        var warnings = new List<RoiValidationWarningDto>();
        foreach (var error in result.Errors)
        {
            if (error is RoiValidationWarningError warningError)
            {
                warnings.Add(warningError.Warning);
                continue;
            }

            var code = error.Metadata.TryGetValue("code", out var codeValue)
                ? codeValue?.ToString() ?? "validation.error"
                : "validation.error";
            var path = error.Metadata.TryGetValue("path", out var pathValue)
                ? pathValue?.ToString() ?? "root"
                : "root";
            warnings.Add(new RoiValidationWarningDto(code, error.Message, path));
        }

        return warnings;
    }
}

public interface IRoiValidationRuleEvaluator
{
    string RuleType { get; }
    string Label { get; }
    string Description { get; }
    IReadOnlySet<ERoiType> SupportedRoiTypes { get; }
    JsonElement DefaultParams { get; }

    Result ValidateParams(JsonElement? parameters);

    Result Evaluate(
        RoiValidationRuleEvaluationContext context,
        JsonElement? parameters,
        string path
    );
}

public interface IRoiValidationRuleRegistry
{
    bool TryGet(string ruleType, out IRoiValidationRuleEvaluator evaluator);
    IReadOnlyList<IRoiValidationRuleEvaluator> GetAll();
}

public interface IRoiValidationRuleCatalogProvider
{
    RoiValidationRuleCatalogDto GetCatalog();
}

public interface IRoiValidationConditionTreeValidator
{
    Result Validate(
        ERoiType roiType,
        LogsheetXtractor.Domain.ValueObjects.RoiValidation.RoiValidationConditionNode conditionRoot
    );
}

public interface IRoiValidationConditionEvaluator
{
    IReadOnlyList<RoiValidationWarningDto> Evaluate(
        ERoiType roiType,
        string? value,
        LogsheetXtractor.Domain.ValueObjects.RoiValidation.RoiValidationConditionNode? conditionRoot
    );
}

public interface IRoiValidationConditionNodeEvaluator
{
    bool CanHandle(
        LogsheetXtractor.Domain.ValueObjects.RoiValidation.RoiValidationConditionNode node
    );

    Result Evaluate(
        LogsheetXtractor.Domain.ValueObjects.RoiValidation.RoiValidationConditionNode node,
        RoiValidationRuleEvaluationContext context,
        string path,
        Func<
            LogsheetXtractor.Domain.ValueObjects.RoiValidation.RoiValidationConditionNode,
            string,
            Result
        > evaluateChild
    );
}
