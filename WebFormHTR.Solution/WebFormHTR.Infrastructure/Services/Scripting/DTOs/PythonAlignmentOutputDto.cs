using System.Text.Json.Serialization;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Infrastructure.Services.Scripting.DTOs;

public class PythonAlignmentOutputDto
{
    [JsonPropertyName("frontside")] public PythonAlignmentData Frontside { get; init; } = new();
    [JsonPropertyName("backside")] public PythonAlignmentData? Backside { get; init; }
}

public class PythonAlignmentData
{
    [JsonPropertyName("templatePoints")] public List<PointCoordinate> TemplatePoints { get; init; } = [];

    [JsonPropertyName("targetPoints")] public List<PointCoordinate> TargetPoints { get; init; } = [];
}