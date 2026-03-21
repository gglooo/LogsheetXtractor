using System.Text.Json.Serialization;
using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Infrastructure.Services.Scripting.DTOs;

public class PythonAlignmentConfig
{
    [JsonPropertyName("template_points")]
    public List<int[]> TemplatePoints { get; private set; }

    [JsonPropertyName("target_points")]
    public List<int[]> TargetPoints { get; private set; }

    public PythonAlignmentConfig(
        IEnumerable<PointCoordinate> templatePoints,
        IEnumerable<PointCoordinate> targetPoints
    )
    {
        TemplatePoints = templatePoints.Select(p => new[] { p.X, p.Y }).ToList();
        TargetPoints = targetPoints.Select(p => new[] { p.X, p.Y }).ToList();
    }
}
