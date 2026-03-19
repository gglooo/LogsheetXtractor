using System.Text.Json;
using FluentResults;
using WebFormHTR.Application.Errors;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.RoiValidation.Rules;

file sealed record NumberRangeParams(decimal? Min, decimal? Max, bool InclusiveMin = true, bool InclusiveMax = true);

public sealed class NumberRangeRuleEvaluator : RoiValidationRuleEvaluatorBase
{
    public override string RuleType => "number.range";
    public override string Label => "Number range";
    public override string Description => "Numeric value must be inside configured range.";
    public override IReadOnlySet<ERoiType> SupportedRoiTypes => new HashSet<ERoiType> { ERoiType.Number };
    public override JsonElement DefaultParams => RoiValidationJson.ParseDefault("{\"min\":0,\"max\":100,\"inclusiveMin\":true,\"inclusiveMax\":true}");

    public override Result ValidateParams(JsonElement? parameters)
    {
        var parsed = RoiValidationJson.Deserialize<NumberRangeParams>(parameters);
        if (parsed.IsFailed)
        {
            return parsed.ToResult();
        }

        var p = parsed.Value;
        if (p.Min is null && p.Max is null)
        {
            return Result.Fail(new ValidationError("number.range requires at least one boundary (min or max)."));
        }

        if (p.Min is not null && p.Max is not null && p.Min > p.Max)
        {
            return Result.Fail(new ValidationError("number.range min must be less than or equal to max."));
        }

        return Result.Ok();
    }

    public override Result Evaluate(
        RoiValidationRuleEvaluationContext context,
        JsonElement? parameters,
        string path)
    {
        var parsed = RoiValidationJson.Deserialize<NumberRangeParams>(parameters);
        if (parsed.IsFailed)
        {
            return Result.Fail(new RoiValidationWarningError(RuleType, "Invalid number.range parameters.", path));
        }

        if (!RoiValidationValueParsing.TryParseNumber(context.Value, out var number))
        {
            return Result.Fail(new RoiValidationWarningError(RuleType, "Value is not a valid number.", path));
        }

        var p = parsed.Value;
        if (p.Min is not null)
        {
            var minOk = p.InclusiveMin ? number >= p.Min : number > p.Min;
            if (!minOk)
            {
                return Result.Fail(new RoiValidationWarningError(RuleType, $"Value must be greater than {(p.InclusiveMin ? "or equal to " : string.Empty)}{p.Min}.", path));
            }
        }

        if (p.Max is not null)
        {
            var maxOk = p.InclusiveMax ? number <= p.Max : number < p.Max;
            if (!maxOk)
            {
                return Result.Fail(new RoiValidationWarningError(RuleType, $"Value must be less than {(p.InclusiveMax ? "or equal to " : string.Empty)}{p.Max}.", path));
            }
        }

        return Result.Ok();
    }
}
