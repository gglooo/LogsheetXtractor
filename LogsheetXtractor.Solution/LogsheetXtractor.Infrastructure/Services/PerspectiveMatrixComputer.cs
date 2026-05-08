using LogsheetXtractor.Infrastructure.Services;
using SkiaSharp;

namespace LogsheetXtractor.Infrastructure.Services;

public class PerspectiveMatrixComputer : IPerspectiveMatrixComputer
{
    public SKMatrix ComputePerspectiveMatrix(SKPoint[] src, SKPoint[] dst)
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
            Persp2 = 1.0f,
        };
    }
}
