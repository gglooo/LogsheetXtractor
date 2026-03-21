using System.Text.Json.Serialization;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Application.Features.Scripting.DTOs;

public class ExportLogsheetPayloadDto
{
    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("data")]
    public IEnumerable<ExportLogsheetDataDto> Data { get; set; } = [];
}
