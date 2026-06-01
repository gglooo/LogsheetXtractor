using System.Text.Json.Serialization;

namespace LogsheetXtractor.Infrastructure.Services.Scripting.DTOs;

public class PythonTemplateConfig
{
    [JsonPropertyName("content")]
    public IEnumerable<PythonRoiDto> Rois { get; set; } = [];

    [JsonPropertyName("to_ignore")]
    public IEnumerable<PythonResidualDto> Residuals { get; set; } = [];

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}
