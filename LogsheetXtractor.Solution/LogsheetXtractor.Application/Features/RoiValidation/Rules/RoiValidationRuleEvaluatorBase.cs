using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Domain.Enums;

namespace LogsheetXtractor.Application.Features.RoiValidation.Rules;

internal static class RoiValidationJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static JsonElement ParseDefault(string json)
    {
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    public static Result<T> Deserialize<T>(JsonElement? parameters)
    {
        try
        {
            var json =
                parameters is null
                || parameters.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
                    ? "{}"
                    : parameters.Value.GetRawText();

            var parsed = JsonSerializer.Deserialize<T>(json, Options);
            if (parsed is null)
            {
                return Result.Fail<T>(
                    new ValidationError("Rule parameters are missing or invalid.")
                );
            }

            return Result.Ok(parsed);
        }
        catch (Exception ex)
        {
            return Result.Fail<T>(
                new ValidationError($"Failed to parse rule parameters: {ex.Message}")
            );
        }
    }
}

internal static class RoiValidationValueParsing
{
    public static bool IsWhitespaceEmpty(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return true;
        }

        return Regex.Replace(input, @"\s+", string.Empty).Length == 0;
    }

    public static string NormalizeText(string? input)
    {
        return (input ?? string.Empty).Trim();
    }

    public static bool TryParseNumber(string? input, out decimal value)
    {
        value = default;
        var normalized = (input ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        normalized = normalized.Replace(" ", string.Empty);

        if (
            decimal.TryParse(
                normalized,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out value
            )
        )
        {
            return true;
        }

        var withDotSeparator = normalized.Replace(',', '.');
        return decimal.TryParse(
            withDotSeparator,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out value
        );
    }

    public static int GetDecimalScale(decimal value)
    {
        var bits = decimal.GetBits(value);
        return (bits[3] >> 16) & 0x7F;
    }
}

public abstract class RoiValidationRuleEvaluatorBase : IRoiValidationRuleEvaluator
{
    public abstract string RuleType { get; }
    public abstract string Label { get; }
    public abstract string Description { get; }
    public abstract IReadOnlySet<ERoiType> SupportedRoiTypes { get; }
    public abstract JsonElement DefaultParams { get; }

    public abstract Result ValidateParams(JsonElement? parameters);

    public abstract Result Evaluate(
        RoiValidationRuleEvaluationContext context,
        JsonElement? parameters,
        string path
    );
}
