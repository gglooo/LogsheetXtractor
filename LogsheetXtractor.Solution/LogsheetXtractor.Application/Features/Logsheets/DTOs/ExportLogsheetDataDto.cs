using System.Text.Json.Serialization;
using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Application.Features.Logsheets.DTOs;

/// <summary>
/// One extracted ROI value prepared for the export script.
/// </summary>
public class ExportLogsheetDataDto
{
    /// <summary>The coordinates of the ROI that produced the value.</summary>
    [JsonPropertyName("coordinates")]
    public ExportCoordinateDto Coordinates { get; set; } = new();

    /// <summary>The ROI variable name.</summary>
    [JsonPropertyName("varname")]
    public string VariableName { get; set; } = string.Empty;

    /// <summary>The extracted text.</summary>
    [JsonPropertyName("content")]
    public string Value { get; set; } = string.Empty;

    /// <summary>The page index containing the ROI.</summary>
    [JsonPropertyName("page")]
    public int Page { get; set; } = 0;
}

/// <summary>
/// Coordinates and dimensions of an ROI in an export payload.
/// </summary>
public class ExportCoordinateDto
{
    /// <summary>The horizontal coordinate.</summary>
    [JsonPropertyName("x")]
    public int X { get; set; }

    /// <summary>The vertical coordinate.</summary>
    [JsonPropertyName("y")]
    public int Y { get; set; }

    /// <summary>The ROI width.</summary>
    [JsonPropertyName("width")]
    public int Width { get; set; }

    /// <summary>The ROI height.</summary>
    [JsonPropertyName("height")]
    public int Height { get; set; }
}
