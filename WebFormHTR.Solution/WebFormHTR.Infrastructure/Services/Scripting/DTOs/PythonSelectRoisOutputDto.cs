using System.Text.Json.Serialization;

namespace WebFormHTR.Infrastructure.Services.Scripting.DTOs;

public class PythonSelectRoisOutputDto
{
    [JsonPropertyName("content")] public List<PythonRoiDto> Content { get; set; } = [];

    [JsonPropertyName("height")] public int Height { get; set; }

    [JsonPropertyName("width")] public int Width { get; set; }
}