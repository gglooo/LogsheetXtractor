namespace WebFormHTR.Domain.ValueObjects;

public record AlignmentContainer(
    List<PointCoordinate>? Frontside,
    List<PointCoordinate>? Backside);

public record PointCoordinate(int X, int Y);