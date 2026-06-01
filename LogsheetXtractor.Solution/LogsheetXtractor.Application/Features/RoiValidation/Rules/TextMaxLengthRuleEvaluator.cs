using System.Text.Json;
using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Domain.Enums;

namespace LogsheetXtractor.Application.Features.RoiValidation.Rules;

file sealed record MaxLengthParams(int Max);

public sealed class TextMaxLengthRuleEvaluator : RoiValidationRuleEvaluatorBase
{
    public override string RuleType => "text.maxLength";
    public override string Label => "Max length";
    public override string Description => "Text length must be at most N.";
    public override IReadOnlySet<ERoiType> SupportedRoiTypes =>
        new HashSet<ERoiType> { ERoiType.Handwritten, ERoiType.Barcode };
    public override JsonElement DefaultParams => RoiValidationJson.ParseDefault("{\"max\":8}");

    public override Result ValidateParams(JsonElement? parameters)
    {
        var parsed = RoiValidationJson.Deserialize<MaxLengthParams>(parameters);
        if (parsed.IsFailed)
        {
            return parsed.ToResult();
        }

        return parsed.Value.Max < 0
            ? Result.Fail(new ValidationError("text.maxLength max must be non-negative."))
            : Result.Ok();
    }

    public override Result Evaluate(
        RoiValidationRuleEvaluationContext context,
        JsonElement? parameters,
        string path
    )
    {
        var parsed = RoiValidationJson.Deserialize<MaxLengthParams>(parameters);
        if (parsed.IsFailed)
        {
            return Result.Fail(
                new RoiValidationWarningError(RuleType, "Invalid text.maxLength parameters.", path)
            );
        }

        var normalized = RoiValidationValueParsing.NormalizeText(context.Value);
        return normalized.Length <= parsed.Value.Max
            ? Result.Ok()
            : Result.Fail(
                new RoiValidationWarningError(
                    RuleType,
                    $"Text must be at most {parsed.Value.Max} characters long.",
                    path
                )
            );
    }
}
