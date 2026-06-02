using System.Text.Json.Serialization;

namespace LogsheetXtractor.Application.Features.Scripting.DTOs;

/// <summary>
/// TODO-DOC: Describe PdfDimensionsDto purpose and usage.
/// TODO-DOC-MEMBERS: Document public properties.
/// </summary>
public class PdfDimensionsDto
{
    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}
