using System.Text.Json;
using FluentResults;
using WebFormHTR.Application.Errors;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.RoiValidation.Rules;

file sealed record PrefixParams(string Prefix);

public sealed class TextPrefixRuleEvaluator : RoiValidationRuleEvaluatorBase
{
    public override string RuleType => "text.prefix";
    public override string Label => "Prefix";
    public override string Description => "Text must start with prefix.";
    public override IReadOnlySet<ERoiType> SupportedRoiTypes => new HashSet<ERoiType> { ERoiType.Barcode };
    public override JsonElement DefaultParams => RoiValidationJson.ParseDefault("{\"prefix\":\"\"}");

    public override Result ValidateParams(JsonElement? parameters)
    {
        var parsed = RoiValidationJson.Deserialize<PrefixParams>(parameters);
        if (parsed.IsFailed)
        {
            return parsed.ToResult();
        }

        return string.IsNullOrEmpty(parsed.Value.Prefix)
            ? Result.Fail(new ValidationError("text.prefix prefix is required."))
            : Result.Ok();
    }

    public override Result Evaluate(RoiValidationRuleEvaluationContext context, JsonElement? parameters, string path)
    {
        var parsed = RoiValidationJson.Deserialize<PrefixParams>(parameters);
        if (parsed.IsFailed)
        {
            return Result.Fail(new RoiValidationWarningError(RuleType, "Invalid text.prefix parameters.", path));
        }

        var input = RoiValidationValueParsing.NormalizeText(context.Value);
        return input.StartsWith(parsed.Value.Prefix, StringComparison.Ordinal)
            ? Result.Ok()
            : Result.Fail(new RoiValidationWarningError(RuleType, $"Text must start with '{parsed.Value.Prefix}'.", path));
    }
}
