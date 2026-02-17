using SkiaSharp;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace WebFormHTR.Infrastructure.Services.Coordinates;

public class CoordinateTransformerService(
    IPerspectiveMatrixComputer perspectiveMatrixComputer,
    ILogger<CoordinateTransformerService> logger)
    : ICoordinateTransformerService
{
    public Domain.ValueObjects.Coordinates TransformCoordinates(
        Domain.ValueObjects.Coordinates destinationCoordinates,
        Domain.ValueObjects.Coordinates sourceCoordinates,
        List<PointCoordinate>? alignmentPoints,
        double renderScaleFactor)
    {
        if (alignmentPoints is not { Count: 4 })
        {
            logger.LogWarning(
                "Adjustment points invalid or missing. Returning scaled original coordinates. Count: {Count}",
                alignmentPoints?.Count);
            return new Domain.ValueObjects.Coordinates(
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
            new(sourceCoordinates.X + sourceCoordinates.Width,
                sourceCoordinates.Y + sourceCoordinates.Height),
            new(sourceCoordinates.X, sourceCoordinates.Y + sourceCoordinates.Height)
        };

        var dstPoints = alignmentPoints
            .Select(p => new SKPoint(p.X, p.Y))
            .ToArray();

        var matrix = perspectiveMatrixComputer.ComputePerspectiveMatrix(srcPoints, dstPoints);

        var roiCorners = new SKPoint[]
        {
            new(destinationCoordinates.X, destinationCoordinates.Y),
            new(destinationCoordinates.X + destinationCoordinates.Width, destinationCoordinates.Y),
            new(destinationCoordinates.X + destinationCoordinates.Width,
                destinationCoordinates.Y + destinationCoordinates.Height),
            new(destinationCoordinates.X, destinationCoordinates.Y + destinationCoordinates.Height)
        };

        var transformedCorners = matrix.MapPoints(roiCorners);

        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

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

        return new Domain.ValueObjects.Coordinates(
            finalX,
            finalY,
            finalMaxX - finalX,
            finalMaxY - finalY
        );
    }
}