using System.Text.Json.Serialization;

namespace WebFormHTR.Infrastructure.Services.Scripting.DTOs;

public class PythonSelectRoisOutputDto
{
    [JsonPropertyName("content")] public List<PythonRoiDto> Content { get; set; } = [];

    [JsonPropertyName("to_ignore")] public List<PythonResidualDto> ToIgnore { get; set; } = [];

    [JsonPropertyName("height")] public float Height { get; set; }

    [JsonPropertyName("width")] public float Width { get; set; }
}