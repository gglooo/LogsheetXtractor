using System.Text.Json;
using FluentResults;
using WebFormHTR.Application.Errors;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.RoiValidation.Rules;

file sealed record MinLengthParams(int Min);

public sealed class TextMinLengthRuleEvaluator : RoiValidationRuleEvaluatorBase
{
    public override string RuleType => "text.minLength";
    public override string Label => "Min length";
    public override string Description => "Text length must be at least N.";
    public override IReadOnlySet<ERoiType> SupportedRoiTypes => new HashSet<ERoiType> { ERoiType.Handwritten, ERoiType.Barcode };
    public override JsonElement DefaultParams => RoiValidationJson.ParseDefault("{\"min\":1}");

    public override Result ValidateParams(JsonElement? parameters)
    {
        var parsed = RoiValidationJson.Deserialize<MinLengthParams>(parameters);
        if (parsed.IsFailed)
        {
            return parsed.ToResult();
        }

        return parsed.Value.Min < 0
            ? Result.Fail(new ValidationError("text.minLength min must be non-negative."))
            : Result.Ok();
    }

    public override Result Evaluate(RoiValidationRuleEvaluationContext context, JsonElement? parameters, string path)
    {
        var parsed = RoiValidationJson.Deserialize<MinLengthParams>(parameters);
        if (parsed.IsFailed)
        {
            return Result.Fail(new RoiValidationWarningError(RuleType, "Invalid text.minLength parameters.", path));
        }

        var normalized = RoiValidationValueParsing.NormalizeText(context.Value);
        return normalized.Length >= parsed.Value.Min
            ? Result.Ok()
            : Result.Fail(new RoiValidationWarningError(RuleType, $"Text must be at least {parsed.Value.Min} characters long.", path));
    }
}
