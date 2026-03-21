namespace LogsheetXtractor.Application.Features.Logsheets.DTOs;

public record AlignmentDataDto(
    List<PointCoordinateDto>? Frontside,
    List<PointCoordinateDto>? Backside
);

public class PointCoordinateDto
{
    public int X { get; set; }
    public int Y { get; set; }
}
