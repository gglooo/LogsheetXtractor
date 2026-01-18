using SkiaSharp;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Infrastructure.Services.Coordinates;

public class CoordinateTransformerService : ICoordinateTransformerService
{
    public CoordinateTransformerService(IPerspectiveMatrixComputer perspectiveMatrixComputer)
    {
        _perspectiveMatrixComputer = perspectiveMatrixComputer;
    }

    private readonly IPerspectiveMatrixComputer _perspectiveMatrixComputer;

    public Domain.ValueObjects.Coordinates TransformCoordinates(
        Domain.ValueObjects.Coordinates destinationCoordinates,
        Domain.ValueObjects.Coordinates sourceCoordinates,
        List<PointCoordinate>? alignmentPoints,
        double renderScaleFactor)
    {
        if (alignmentPoints is not { Count: 4 })
        {
            return new Domain.ValueObjects.Coordinates
            {
                X = (int)(destinationCoordinates.X * renderScaleFactor),
                Y = (int)(destinationCoordinates.Y * renderScaleFactor),
                Width = (int)(destinationCoordinates.Width * renderScaleFactor),
                Height = (int)(destinationCoordinates.Height * renderScaleFactor)
            };
        }

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

        var matrix = _perspectiveMatrixComputer.ComputePerspectiveMatrix(srcPoints, dstPoints);

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

        return new Domain.ValueObjects.Coordinates
        {
            X = finalX,
            Y = finalY,
            Width = finalMaxX - finalX,
            Height = finalMaxY - finalY
        };
    }
}