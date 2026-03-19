using System.Text.Json;

namespace WebFormHTR.Domain.ValueObjects.RoiValidation;

public sealed record RoiValidationConditionNode
{
    public required string Type { get; init; }
    public string? Operator { get; init; }
    public IReadOnlyList<RoiValidationConditionNode>? Children { get; init; }
    public string? RuleType { get; init; }
    public JsonElement? Params { get; init; }
    public int? SchemaVersion { get; init; }
}
