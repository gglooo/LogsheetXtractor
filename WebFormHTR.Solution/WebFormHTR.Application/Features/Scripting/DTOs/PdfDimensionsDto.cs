using System.Text.Json.Serialization;

namespace WebFormHTR.Application.Features.Scripting.DTOs;

public class PdfDimensionsDto
{
    [JsonPropertyName("width")] public float Width { get; set; }

    [JsonPropertyName("height")] public float Height { get; set; }
}