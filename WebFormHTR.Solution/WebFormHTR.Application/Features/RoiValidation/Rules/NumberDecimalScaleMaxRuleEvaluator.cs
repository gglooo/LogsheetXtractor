using System.Text.Json;
using FluentResults;
using WebFormHTR.Application.Errors;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.RoiValidation.Rules;

file sealed record NumberDecimalScaleMaxParams(int Max);

public sealed class NumberDecimalScaleMaxRuleEvaluator : RoiValidationRuleEvaluatorBase
{
    public override string RuleType => "number.decimalScaleMax";
    public override string Label => "Max decimal scale";
    public override string Description => "Numeric value can have at most N decimal places.";
    public override IReadOnlySet<ERoiType> SupportedRoiTypes => new HashSet<ERoiType> { ERoiType.Number };
    public override JsonElement DefaultParams => RoiValidationJson.ParseDefault("{\"max\":2}");

    public override Result ValidateParams(JsonElement? parameters)
    {
        var parsed = RoiValidationJson.Deserialize<NumberDecimalScaleMaxParams>(parameters);
        if (parsed.IsFailed)
        {
            return parsed.ToResult();
        }

        return parsed.Value.Max < 0
            ? Result.Fail(new ValidationError("number.decimalScaleMax max must be non-negative."))
            : Result.Ok();
    }

    public override Result Evaluate(
        RoiValidationRuleEvaluationContext context,
        JsonElement? parameters,
        string path)
    {
        var parsed = RoiValidationJson.Deserialize<NumberDecimalScaleMaxParams>(parameters);
        if (parsed.IsFailed)
        {
            return Result.Fail(new RoiValidationWarningError(RuleType, "Invalid number.decimalScaleMax parameters.", path));
        }

        if (!RoiValidationValueParsing.TryParseNumber(context.Value, out var number))
        {
            return Result.Fail(new RoiValidationWarningError(RuleType, "Value is not a valid number.", path));
        }

        return RoiValidationValueParsing.GetDecimalScale(number) <= parsed.Value.Max
            ? Result.Ok()
            : Result.Fail(new RoiValidationWarningError(RuleType, $"Value must have at most {parsed.Value.Max} decimal places.", path));
    }
}
