using System.Text.Json.Serialization;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Application.Features.Scripting.DTOs;

/// <summary>
/// Payload passed to the export script containing page dimensions and extracted ROI data.
/// </summary>
public class ExportLogsheetPayloadDto
{
    /// <summary>The page width used by the export script.</summary>
    [JsonPropertyName("width")]
    public int Width { get; set; }

    /// <summary>The page height used by the export script.</summary>
    [JsonPropertyName("height")]
    public int Height { get; set; }

    /// <summary>The extracted values and their ROI coordinates.</summary>
    [JsonPropertyName("data")]
    public IEnumerable<ExportLogsheetDataDto> Data { get; set; } = [];
}
