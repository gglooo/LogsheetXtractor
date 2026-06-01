using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace LogsheetXtractor.Infrastructure.Services.Coordinates;

public class CoordinateTransformerService(
    IPerspectiveMatrixComputer perspectiveMatrixComputer,
    ILogger<CoordinateTransformerService> logger
) : ICoordinateTransformerService
{
    public LogsheetXtractor.Domain.ValueObjects.Coordinates TransformCoordinates(
        LogsheetXtractor.Domain.ValueObjects.Coordinates destinationCoordinates,
        LogsheetXtractor.Domain.ValueObjects.Coordinates sourceCoordinates,
        List<PointCoordinate>? alignmentPoints,
        double renderScaleFactor
    )
    {
        if (alignmentPoints is not { Count: 4 })
        {
            logger.LogWarning(
                "Adjustment points invalid or missing. Returning scaled original coordinates. Count: {Count}",
                alignmentPoints?.Count
            );
            return new LogsheetXtractor.Domain.ValueObjects.Coordinates(
                (int)(destinationCoordinates.X * renderScaleFactor),
                (int)(destinationCoordinates.Y * renderScaleFactor),
                (int)(destinationCoordinates.Width * renderScaleFactor),
                (int)(destinationCoordinates.Height * renderScaleFactor)
            );
        }

        logger.LogDebug("Transforming coordinates with perspective matrix.");

        var srcPoints = new SKPoint[]
        {
            new(sourceCoordinates.X, sourceCoordinates.Y),
            new(sourceCoordinates.X + sourceCoordinates.Width, sourceCoordinates.Y),
            new(
                sourceCoordinates.X + sourceCoordinates.Width,
                sourceCoordinates.Y + sourceCoordinates.Height
            ),
            new(sourceCoordinates.X, sourceCoordinates.Y + sourceCoordinates.Height),
        };

        var dstPoints = alignmentPoints.Select(p => new SKPoint(p.X, p.Y)).ToArray();

        var matrix = perspectiveMatrixComputer.ComputePerspectiveMatrix(srcPoints, dstPoints);

        var roiCorners = new SKPoint[]
        {
            new(destinationCoordinates.X, destinationCoordinates.Y),
            new(destinationCoordinates.X + destinationCoordinates.Width, destinationCoordinates.Y),
            new(
                destinationCoordinates.X + destinationCoordinates.Width,
                destinationCoordinates.Y + destinationCoordinates.Height
            ),
            new(destinationCoordinates.X, destinationCoordinates.Y + destinationCoordinates.Height),
        };

        var transformedCorners = matrix.MapPoints(roiCorners);

        float minX = float.MaxValue,
            minY = float.MaxValue;
        float maxX = float.MinValue,
            maxY = float.MinValue;

        foreach (var p in transformedCorners)
        {
            if (p.X < minX)
            {
                minX = p.X;
            }

            if (p.Y < minY)
            {
                minY = p.Y;
            }

            if (p.X > maxX)
            {
                maxX = p.X;
            }

            if (p.Y > maxY)
            {
                maxY = p.Y;
            }
        }

        var finalX = (int)Math.Floor(minX * renderScaleFactor);
        var finalY = (int)Math.Floor(minY * renderScaleFactor);
        var finalMaxX = (int)Math.Ceiling(maxX * renderScaleFactor);
        var finalMaxY = (int)Math.Ceiling(maxY * renderScaleFactor);

        return new LogsheetXtractor.Domain.ValueObjects.Coordinates(
            finalX,
            finalY,
            finalMaxX - finalX,
            finalMaxY - finalY
        );
    }

    public List<PointCoordinate> NormalizeAlignmentPoints(
        List<PointCoordinate> rawTemplatePoints,
        List<PointCoordinate> rawTargetPoints,
        int templateWidth,
        int templateHeight,
        int imageWidth,
        int imageHeight
    )
    {
        var matrix = perspectiveMatrixComputer.ComputePerspectiveMatrix(
            rawTemplatePoints.Select(p => new SKPoint(p.X, p.Y)).ToArray(),
            rawTargetPoints.Select(p => new SKPoint(p.X, p.Y)).ToArray()
        );

        var standardCorners = new SKPoint[]
        {
            new(0, 0),
            new(imageWidth, 0),
            new(imageWidth, imageHeight),
            new(0, imageHeight),
        };

        var scaleX = (double)templateWidth / imageWidth;
        var scaleY = (double)templateHeight / imageHeight;

        var normalizedPoints = matrix
            .MapPoints(standardCorners)
            .Select(p => new PointCoordinate(
                (int)Math.Round(p.X * scaleX),
                (int)Math.Round(p.Y * scaleY)
            ))
            .ToList();

        return normalizedPoints;
    }
}
