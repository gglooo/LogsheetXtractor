using SkiaSharp;

namespace LogsheetXtractor.Infrastructure.Services;

public interface IPerspectiveMatrixComputer
{
    SKMatrix ComputePerspectiveMatrix(SKPoint[] src, SKPoint[] dst);
}
