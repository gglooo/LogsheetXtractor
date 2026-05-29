using System.Text.Json.Serialization;

namespace LogsheetXtractor.Infrastructure.Services.Scripting.DTOs;

public class PythonSelectRoisOutputDto
{
    [JsonPropertyName("content")]
    public List<PythonRoiDto> Content { get; set; } = [];

    [JsonPropertyName("to_ignore")]
    public List<PythonResidualDto> ToIgnore { get; set; } = [];

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }
}
