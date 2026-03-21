using System.Text.Json;
using System.Text.RegularExpressions;
using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Domain.Enums;

namespace LogsheetXtractor.Application.Features.RoiValidation.Rules;

file sealed record NotRegexParams(string Pattern, int? MaxInputLength);

public sealed class TextNotRegexRuleEvaluator : RoiValidationRuleEvaluatorBase
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(200);

    public override string RuleType => "text.notRegex";
    public override string Label => "Not regex";
    public override string Description => "Text must not match regex pattern.";
    public override IReadOnlySet<ERoiType> SupportedRoiTypes =>
        new HashSet<ERoiType> { ERoiType.Handwritten, ERoiType.Barcode };
    public override JsonElement DefaultParams =>
        RoiValidationJson.ParseDefault("{\"pattern\":\"\"}");

    public override Result ValidateParams(JsonElement? parameters)
    {
        var parsed = RoiValidationJson.Deserialize<NotRegexParams>(parameters);
        if (parsed.IsFailed)
        {
            return parsed.ToResult();
        }

        if (string.IsNullOrWhiteSpace(parsed.Value.Pattern))
        {
            return Result.Fail(new ValidationError("text.notRegex pattern is required."));
        }

        try
        {
            _ = new Regex(parsed.Value.Pattern, RegexOptions.None, RegexTimeout);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(
                new ValidationError($"text.notRegex pattern is invalid: {ex.Message}")
            );
        }
    }

    public override Result Evaluate(
        RoiValidationRuleEvaluationContext context,
        JsonElement? parameters,
        string path
    )
    {
        var parsed = RoiValidationJson.Deserialize<NotRegexParams>(parameters);
        if (parsed.IsFailed || string.IsNullOrWhiteSpace(parsed.Value.Pattern))
        {
            return Result.Fail(
                new RoiValidationWarningError(RuleType, "Invalid text.notRegex parameters.", path)
            );
        }

        var input = context.Value ?? string.Empty;
        if (parsed.Value.MaxInputLength is not null && input.Length > parsed.Value.MaxInputLength)
        {
            return Result.Fail(
                new RoiValidationWarningError(
                    RuleType,
                    $"Text is too long for regex evaluation (>{parsed.Value.MaxInputLength}).",
                    path
                )
            );
        }

        try
        {
            var regex = new Regex(parsed.Value.Pattern, RegexOptions.None, RegexTimeout);
            return regex.IsMatch(input)
                ? Result.Fail(
                    new RoiValidationWarningError(RuleType, "Text matches forbidden pattern.", path)
                )
                : Result.Ok();
        }
        catch
        {
            return Result.Fail(
                new RoiValidationWarningError(RuleType, "Regex evaluation failed.", path)
            );
        }
    }
}
