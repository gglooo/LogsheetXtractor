using System.Text.Json;
using FluentResults;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.RoiValidation.Rules;

public sealed class NumberIntegerOnlyRuleEvaluator : RoiValidationRuleEvaluatorBase
{
    public override string RuleType => "number.integerOnly";
    public override string Label => "Integer only";
    public override string Description => "Numeric value must be an integer.";
    public override IReadOnlySet<ERoiType> SupportedRoiTypes => new HashSet<ERoiType> { ERoiType.Number };
    public override JsonElement DefaultParams => RoiValidationJson.ParseDefault("{}");

    public override Result ValidateParams(JsonElement? parameters)
    {
        return Result.Ok();
    }

    public override Result Evaluate(
        RoiValidationRuleEvaluationContext context,
        JsonElement? parameters,
        string path)
    {
        if (!RoiValidationValueParsing.TryParseNumber(context.Value, out var number))
        {
            return Result.Fail(new RoiValidationWarningError(RuleType, "Value is not a valid number.", path));
        }

        return decimal.Truncate(number) == number
            ? Result.Ok()
            : Result.Fail(new RoiValidationWarningError(RuleType, "Value must be an integer.", path));
    }
}
