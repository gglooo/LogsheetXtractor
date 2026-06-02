using System.Text.Json.Serialization;
using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Application.Features.Logsheets.DTOs;

/// <summary>
/// TODO-DOC: Describe ExportLogsheetDataDto purpose and usage.
/// TODO-DOC-MEMBERS: Document public properties.
/// </summary>
public class ExportLogsheetDataDto
{
    [JsonPropertyName("coordinates")]
    public ExportCoordinateDto Coordinates { get; set; } = new();

    [JsonPropertyName("varname")]
    public string VariableName { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("page")]
    public int Page { get; set; } = 0;
}

/// <summary>
/// TODO-DOC: Describe ExportCoordinateDto purpose and usage.
/// TODO-DOC-MEMBERS: Document public properties.
/// </summary>
public class ExportCoordinateDto
{
    [JsonPropertyName("x")]
    public int X { get; set; }

    [JsonPropertyName("y")]
    public int Y { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}
