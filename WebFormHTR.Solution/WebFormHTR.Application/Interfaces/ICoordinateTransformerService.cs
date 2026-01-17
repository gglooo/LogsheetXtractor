using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Application.Interfaces;

public interface ICoordinateTransformerService
{
    Coordinates TransformCoordinates(
        Coordinates destinationCoordinates,
        Coordinates sourceCoordinates,
        List<PointCoordinate>? alignmentPoints,
        double renderScaleFactor = 1.0);
}