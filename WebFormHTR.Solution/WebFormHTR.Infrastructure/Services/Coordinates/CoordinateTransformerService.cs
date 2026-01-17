using SkiaSharp;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Infrastructure.Services.Coordinates;

public class CoordinateTransformerService : ICoordinateTransformerService
{
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

        var matrix = ComputePerspectiveMatrix(srcPoints, dstPoints);

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

    // Source: Gemini
    private SKMatrix ComputePerspectiveMatrix(SKPoint[] src, SKPoint[] dst)
    {
        var A = new double[8][];
        for (var i = 0; i < 8; i++)
        {
            A[i] = new double[9];
        }

        for (var i = 0; i < 4; i++)
        {
            var sx = src[i].X;
            var sy = src[i].Y;
            var dx = dst[i].X;
            var dy = dst[i].Y;

            A[2 * i][0] = sx;
            A[2 * i][1] = sy;
            A[2 * i][2] = 1;
            A[2 * i][3] = 0;
            A[2 * i][4] = 0;
            A[2 * i][5] = 0;
            A[2 * i][6] = -sx * dx;
            A[2 * i][7] = -sy * dx;
            A[2 * i][8] = dx;

            A[2 * i + 1][0] = 0;
            A[2 * i + 1][1] = 0;
            A[2 * i + 1][2] = 0;
            A[2 * i + 1][3] = sx;
            A[2 * i + 1][4] = sy;
            A[2 * i + 1][5] = 1;
            A[2 * i + 1][6] = -sx * dy;
            A[2 * i + 1][7] = -sy * dy;
            A[2 * i + 1][8] = dy;
        }

        for (var i = 0; i < 8; i++)
        {
            var maxEl = Math.Abs(A[i][i]);
            var maxRow = i;
            for (var k = i + 1; k < 8; k++)
            {
                if (Math.Abs(A[k][i]) > maxEl)
                {
                    maxEl = Math.Abs(A[k][i]);
                    maxRow = k;
                }
            }

            for (var k = i; k < 9; k++)
            {
                (A[maxRow][k], A[i][k]) = (A[i][k], A[maxRow][k]);
            }

            for (var k = i + 1; k < 8; k++)
            {
                var c = -A[k][i] / A[i][i];
                for (var j = i; j < 9; j++)
                {
                    if (i == j)
                    {
                        A[k][j] = 0;
                    }
                    else
                    {
                        A[k][j] += c * A[i][j];
                    }
                }
            }
        }

        var x = new double[8];
        for (var i = 7; i >= 0; i--)
        {
            x[i] = A[i][8] / A[i][i];
            for (var k = i - 1; k >= 0; k--)
            {
                A[k][8] -= A[k][i] * x[i];
            }
        }

        return new SKMatrix
        {
            ScaleX = (float)x[0],
            SkewX = (float)x[1],
            TransX = (float)x[2],
            SkewY = (float)x[3],
            ScaleY = (float)x[4],
            TransY = (float)x[5],
            Persp0 = (float)x[6],
            Persp1 = (float)x[7],
            Persp2 = 1.0f
        };
    }
}