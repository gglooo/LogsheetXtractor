using System.Text.Json;
using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Domain.Enums;

namespace LogsheetXtractor.Application.Features.RoiValidation.Rules;

file sealed record TextValueSetParams(IReadOnlyList<string> Values);

public sealed class TextAllowedValuesRuleEvaluator : RoiValidationRuleEvaluatorBase
{
    public override string RuleType => "text.allowedValues";
    public override string Label => "Allowed values";
    public override string Description => "Text must be in configured allowed values.";
    public override IReadOnlySet<ERoiType> SupportedRoiTypes =>
        new HashSet<ERoiType> { ERoiType.Handwritten };
    public override JsonElement DefaultParams => RoiValidationJson.ParseDefault("{\"values\":[]}");

    public override Result ValidateParams(JsonElement? parameters)
    {
        var parsed = RoiValidationJson.Deserialize<TextValueSetParams>(parameters);
        if (parsed.IsFailed)
        {
            return parsed.ToResult();
        }

        return parsed.Value.Values.Count == 0
            ? Result.Fail(
                new ValidationError("text.allowedValues requires a non-empty values list.")
            )
            : Result.Ok();
    }

    public override Result Evaluate(
        RoiValidationRuleEvaluationContext context,
        JsonElement? parameters,
        string path
    )
    {
        var parsed = RoiValidationJson.Deserialize<TextValueSetParams>(parameters);
        if (parsed.IsFailed)
        {
            return Result.Fail(
                new RoiValidationWarningError(
                    RuleType,
                    "Invalid text.allowedValues parameters.",
                    path
                )
            );
        }

        var input = RoiValidationValueParsing.NormalizeText(context.Value);
        return parsed.Value.Values.Contains(input, StringComparer.Ordinal)
            ? Result.Ok()
            : Result.Fail(
                new RoiValidationWarningError(RuleType, "Text is not in allowed values.", path)
            );
    }
}
