using System.Text.Json;
using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Domain.Enums;

namespace LogsheetXtractor.Application.Features.RoiValidation.Rules;

file sealed record NumberSetParams(IReadOnlyList<string> Values);

public sealed class NumberNotInSetRuleEvaluator : RoiValidationRuleEvaluatorBase
{
    public override string RuleType => "number.notInSet";
    public override string Label => "Not in set";
    public override string Description => "Value must not equal any configured number.";
    public override IReadOnlySet<ERoiType> SupportedRoiTypes =>
        new HashSet<ERoiType> { ERoiType.Number };
    public override JsonElement DefaultParams => RoiValidationJson.ParseDefault("{\"values\":[]}");

    public override Result ValidateParams(JsonElement? parameters)
    {
        var parsed = RoiValidationJson.Deserialize<NumberSetParams>(parameters);
        if (parsed.IsFailed)
        {
            return parsed.ToResult();
        }

        return parsed.Value.Values.Count == 0
            ? Result.Fail(new ValidationError("number.notInSet requires a non-empty values list."))
            : Result.Ok();
    }

    public override Result Evaluate(
        RoiValidationRuleEvaluationContext context,
        JsonElement? parameters,
        string path
    )
    {
        var parsed = RoiValidationJson.Deserialize<NumberSetParams>(parameters);
        if (parsed.IsFailed)
        {
            return Result.Fail(
                new RoiValidationWarningError(RuleType, "Invalid number.notInSet parameters.", path)
            );
        }

        if (!RoiValidationValueParsing.TryParseNumber(context.Value, out var number))
        {
            return Result.Fail(
                new RoiValidationWarningError(RuleType, "Value is not a valid number.", path)
            );
        }

        foreach (var raw in parsed.Value.Values)
        {
            if (
                RoiValidationValueParsing.TryParseNumber(raw, out var disallowed)
                && number == disallowed
            )
            {
                return Result.Fail(
                    new RoiValidationWarningError(RuleType, "Value is disallowed.", path)
                );
            }
        }

        return Result.Ok();
    }
}
