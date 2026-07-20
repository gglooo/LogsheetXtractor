using System.Text.Json.Serialization;

namespace LogsheetXtractor.Application.Features.Scripting.DTOs;

/// <summary>
/// Page dimensions returned by the PDF-dimensions script.
/// </summary>
public class PdfDimensionsDto
{
    /// <summary>The page width reported by the script.</summary>
    [JsonPropertyName("width")]
    public int Width { get; set; }

    /// <summary>The page height reported by the script.</summary>
    [JsonPropertyName("height")]
    public int Height { get; set; }
}
