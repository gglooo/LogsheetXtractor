using System.Text.Json.Serialization;

namespace WebFormHTR.Infrastructure.Services.Scripting.DTOs;

public class PythonResidualDto
{
    [JsonPropertyName("coords")] public List<int> Coords { get; set; } = [];

    [JsonPropertyName("content")] public string? Content { get; set; }
}