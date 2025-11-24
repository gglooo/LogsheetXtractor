namespace WebFormHTR.Domain.ValueObjects;

public class AlignmentDataModel
{
    public ImageDimensions ReferenceDimensions { get; set; } = new();

    public List<PointCoordinate> TemplatePoints { get; set; } = new();
    public List<PointCoordinate> TargetPoints { get; set; } = new();

    public static AlignmentDataModel FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new AlignmentDataModel();
        }

        return System.Text.Json.JsonSerializer.Deserialize<AlignmentDataModel>(json) ?? new AlignmentDataModel();
    }

    public string ToJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(this);
    }
}

public class ImageDimensions
{
    public int Width { get; set; }
    public int Height { get; set; }
}

public class PointCoordinate
{
    public float X { get; set; }
    public float Y { get; set; }
}