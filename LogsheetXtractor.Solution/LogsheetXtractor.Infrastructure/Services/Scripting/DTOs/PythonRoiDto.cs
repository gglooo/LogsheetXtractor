using LogsheetXtractor.Domain.ValueObjects.RoiValidation;
using System.Text.Json.Serialization;

namespace LogsheetXtractor.Infrastructure.Services.Scripting.DTOs;

public class PythonRoiDto
{
    [JsonPropertyName("coords")]
    public List<int> Coords { get; set; } = [];

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("varname")]
    public string? VarName { get; set; }

    [JsonPropertyName("validation_condition")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RoiValidationConditionNode? ValidationCondition { get; set; }
}
