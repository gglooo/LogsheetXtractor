using System.Text.Json.Serialization;
using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Infrastructure.Services.Scripting.DTOs;

public class PythonAlignmentOutputDto
{
    [JsonPropertyName("frontside")]
    public PythonAlignmentData? Frontside { get; init; }

    [JsonPropertyName("backside")]
    public PythonAlignmentData? Backside { get; init; }
}

public class PythonAlignmentData
{
    [JsonPropertyName("templatePoints")]
    public List<PointCoordinate> TemplatePoints { get; init; } = [];

    [JsonPropertyName("targetPoints")]
    public List<PointCoordinate> TargetPoints { get; init; } = [];

    [JsonPropertyName("imageWidth")]
    public int ImageWidth { get; init; }

    [JsonPropertyName("imageHeight")]
    public int ImageHeight { get; init; }
}
