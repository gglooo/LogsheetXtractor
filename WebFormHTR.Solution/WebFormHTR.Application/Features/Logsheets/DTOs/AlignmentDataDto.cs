namespace WebFormHTR.Application.Features.Logsheets.DTOs;

public record AlignmentDataDto(
    AlignmentDetailDto? Frontside,
    AlignmentDetailDto? Backside
);

public record AlignmentDetailDto(
    DimensionsDto Dimensions,
    List<PointCoordinateDto> TemplatePoints,
    List<PointCoordinateDto> LogsheetPoints
);

public record DimensionsDto(
    int Width,
    int Height
);

public class PointCoordinateDto
{
    public float X { get; set; }
    public float Y { get; set; }
}