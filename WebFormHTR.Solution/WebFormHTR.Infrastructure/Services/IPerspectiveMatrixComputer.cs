using SkiaSharp;

namespace WebFormHTR.Infrastructure.Services;

public interface IPerspectiveMatrixComputer
{
    SKMatrix ComputePerspectiveMatrix(SKPoint[] src, SKPoint[] dst);
}
