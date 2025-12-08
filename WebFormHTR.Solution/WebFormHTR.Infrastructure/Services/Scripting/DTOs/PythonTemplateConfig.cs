using System.Text.Json.Serialization;

namespace WebFormHTR.Infrastructure.Services.Scripting.DTOs;

public class PythonTemplateConfig
{
    [JsonPropertyName("content")] public IEnumerable<PythonRoiDto> Rois { get; set; } = [];
    [JsonPropertyName("to_ignore")] public IEnumerable<PythonResidualDto> Residuals { get; set; } = [];

    [JsonPropertyName("width")] public float Width { get; set; }
    [JsonPropertyName("height")] public float Height { get; set; }
}