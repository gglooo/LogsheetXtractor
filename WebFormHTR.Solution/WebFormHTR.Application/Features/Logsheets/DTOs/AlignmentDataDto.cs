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
    public int X { get; set; }
    public int Y { get; set; }
}