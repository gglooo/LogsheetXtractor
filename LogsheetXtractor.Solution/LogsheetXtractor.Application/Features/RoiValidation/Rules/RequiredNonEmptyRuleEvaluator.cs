using System.Text.Json;
using FluentResults;
using LogsheetXtractor.Domain.Enums;

namespace LogsheetXtractor.Application.Features.RoiValidation.Rules;

public sealed class RequiredNonEmptyRuleEvaluator : RoiValidationRuleEvaluatorBase
{
    public override string RuleType => "common.requiredNonEmpty";
    public override string Label => "Required";
    public override string Description => "Value must not be empty after removing whitespace.";
    public override IReadOnlySet<ERoiType> SupportedRoiTypes =>
        new HashSet<ERoiType> { ERoiType.Handwritten, ERoiType.Number, ERoiType.Barcode };

    public override JsonElement DefaultParams => RoiValidationJson.ParseDefault("{}");

    public override Result ValidateParams(JsonElement? parameters)
    {
        return Result.Ok();
    }

    public override Result Evaluate(
        RoiValidationRuleEvaluationContext context,
        JsonElement? parameters,
        string path
    )
    {
        return RoiValidationValueParsing.IsWhitespaceEmpty(context.Value)
            ? Result.Fail(new RoiValidationWarningError(RuleType, "Value is required.", path))
            : Result.Ok();
    }
}
