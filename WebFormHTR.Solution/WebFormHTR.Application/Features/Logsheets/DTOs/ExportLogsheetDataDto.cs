using System.Text.Json.Serialization;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Application.Features.Logsheets.DTOs;

public class ExportLogsheetDataDto
{
    [JsonPropertyName("coordinates")] public ExportCoordinateDto Coordinates { get; set; } = new();

    [JsonPropertyName("varname")] public string VariableName { get; set; } = string.Empty;
    [JsonPropertyName("content")] public string Value { get; set; } = string.Empty;
}

public class ExportCoordinateDto
{
    [JsonPropertyName("x")] public int X { get; set; }
    [JsonPropertyName("y")] public int Y { get; set; }
    [JsonPropertyName("width")] public int Width { get; set; }
    [JsonPropertyName("height")] public int Height { get; set; }
}