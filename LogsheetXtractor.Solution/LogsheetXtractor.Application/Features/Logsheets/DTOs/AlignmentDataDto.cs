namespace LogsheetXtractor.Application.Features.Logsheets.DTOs;

/// <summary>
/// Alignment points mapping template coordinates to scanned logsheet coordinates.
/// </summary>
public record AlignmentDataDto(
    List<PointCoordinateDto>? Frontside,
    List<PointCoordinateDto>? Backside
);

/// <summary>
/// 2D point coordinate used by alignment data.
/// </summary>
public class PointCoordinateDto
{
    public int X { get; set; }
    public int Y { get; set; }
}
