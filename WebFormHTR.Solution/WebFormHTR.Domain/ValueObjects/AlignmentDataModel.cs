using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebFormHTR.Domain.ValueObjects;

public class AlignmentContainer
{
    [JsonPropertyName("frontside")] public AlignmentDataModel? Frontside { get; set; }

    [JsonPropertyName("backside")] public AlignmentDataModel? Backside { get; set; }

    public static AlignmentContainer FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new AlignmentContainer();
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            return JsonSerializer.Deserialize<AlignmentContainer>(json, options)
                   ?? new AlignmentContainer();
        }
        catch
        {
            return new AlignmentContainer();
        }
    }

    public string ToJson()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        return JsonSerializer.Serialize(this, options);
    }
}

public class AlignmentDataModel
{
    [JsonPropertyName("referenceDimensions")]
    public ImageDimensions ReferenceDimensions { get; set; } = new();

    [JsonPropertyName("templatePoints")] public List<PointCoordinate> TemplatePoints { get; set; } = [];
    [JsonPropertyName("targetPoints")] public List<PointCoordinate> TargetPoints { get; set; } = [];

    public static AlignmentDataModel FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new AlignmentDataModel();
        }

        return JsonSerializer.Deserialize<AlignmentDataModel>(json) ?? new AlignmentDataModel();
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }
}

public class ImageDimensions
{
    [JsonPropertyName("width")] public int Width { get; set; }
    [JsonPropertyName("height")] public int Height { get; set; }
}

public class PointCoordinate
{
    [JsonPropertyName("x")] public float X { get; set; }
    [JsonPropertyName("y")] public float Y { get; set; }
}