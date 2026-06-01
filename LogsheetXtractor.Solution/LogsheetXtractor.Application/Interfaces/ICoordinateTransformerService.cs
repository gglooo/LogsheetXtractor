using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Application.Interfaces;

public interface ICoordinateTransformerService
{
    Coordinates TransformCoordinates(
        Coordinates destinationCoordinates,
        Coordinates sourceCoordinates,
        List<PointCoordinate>? alignmentPoints,
        double renderScaleFactor = 1.0
    );

    List<PointCoordinate> NormalizeAlignmentPoints(
        List<PointCoordinate> rawTemplatePoints,
        List<PointCoordinate> rawTargetPoints,
        int templateWidth,
        int templateHeight,
        int imageWidth,
        int imageHeight
    );
}
