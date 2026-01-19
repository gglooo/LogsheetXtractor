using System.Text.Json.Serialization;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Application.Features.Scripting.DTOs;

public class ExportLogsheetPayloadDto
{
    [JsonPropertyName("width")] public int Width { get; set; }
    [JsonPropertyName("height")] public int Height { get; set; }
    [JsonPropertyName("data")] public IEnumerable<ExportLogsheetDataDto> Data { get; set; } = [];
}