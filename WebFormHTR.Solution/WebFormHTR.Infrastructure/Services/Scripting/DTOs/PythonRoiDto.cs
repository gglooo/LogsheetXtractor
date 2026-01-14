using System.Text.Json.Serialization;

namespace WebFormHTR.Infrastructure.Services.Scripting.DTOs;

public class PythonRoiDto
{
    [JsonPropertyName("coords")] public List<int> Coords { get; set; } = [];

    [JsonPropertyName("type")] public string? Type { get; set; }

    [JsonPropertyName("varname")] public string? VarName { get; set; }
}