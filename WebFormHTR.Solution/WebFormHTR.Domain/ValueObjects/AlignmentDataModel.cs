using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebFormHTR.Domain.ValueObjects;

public class AlignmentContainer
{
    [JsonPropertyName("frontside")] public List<PointCoordinate>? Frontside { get; set; }

    [JsonPropertyName("backside")] public List<PointCoordinate>? Backside { get; set; }

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

public class PointCoordinate
{
    [JsonPropertyName("x")] public int X { get; set; }
    [JsonPropertyName("y")] public int Y { get; set; }
}