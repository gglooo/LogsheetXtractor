using System.Text.Json.Serialization;

namespace WebFormHTR.Application.Features.Scripting.DTOs;

public class PdfDimensionsDto
{
    [JsonPropertyName("width")] public int Width { get; set; }

    [JsonPropertyName("height")] public int Height { get; set; }
}