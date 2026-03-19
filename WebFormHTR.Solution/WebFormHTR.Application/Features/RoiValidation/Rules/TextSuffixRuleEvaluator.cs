using System.Text.Json;
using FluentResults;
using WebFormHTR.Application.Errors;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.RoiValidation.Rules;

file sealed record SuffixParams(string Suffix);

public sealed class TextSuffixRuleEvaluator : RoiValidationRuleEvaluatorBase
{
    public override string RuleType => "text.suffix";
    public override string Label => "Suffix";
    public override string Description => "Text must end with suffix.";
    public override IReadOnlySet<ERoiType> SupportedRoiTypes => new HashSet<ERoiType> { ERoiType.Barcode };
    public override JsonElement DefaultParams => RoiValidationJson.ParseDefault("{\"suffix\":\"\"}");

    public override Result ValidateParams(JsonElement? parameters)
    {
        var parsed = RoiValidationJson.Deserialize<SuffixParams>(parameters);
        if (parsed.IsFailed)
        {
            return parsed.ToResult();
        }

        return string.IsNullOrEmpty(parsed.Value.Suffix)
            ? Result.Fail(new ValidationError("text.suffix suffix is required."))
            : Result.Ok();
    }

    public override Result Evaluate(RoiValidationRuleEvaluationContext context, JsonElement? parameters, string path)
    {
        var parsed = RoiValidationJson.Deserialize<SuffixParams>(parameters);
        if (parsed.IsFailed)
        {
            return Result.Fail(new RoiValidationWarningError(RuleType, "Invalid text.suffix parameters.", path));
        }

        var input = RoiValidationValueParsing.NormalizeText(context.Value);
        return input.EndsWith(parsed.Value.Suffix, StringComparison.Ordinal)
            ? Result.Ok()
            : Result.Fail(new RoiValidationWarningError(RuleType, $"Text must end with '{parsed.Value.Suffix}'.", path));
    }
}
