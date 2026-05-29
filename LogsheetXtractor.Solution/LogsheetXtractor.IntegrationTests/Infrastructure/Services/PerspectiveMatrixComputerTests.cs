using FluentAssertions;
using LogsheetXtractor.Infrastructure.Services;
using SkiaSharp;
using Xunit;

namespace LogsheetXtractor.IntegrationTests.Infrastructure.Services;

public class PerspectiveMatrixComputerTests
{
    private readonly PerspectiveMatrixComputer _computer = new();

    [Fact]
    public void ComputePerspectiveMatrix_ShouldReturnIdentity_WhenPointsAreMatches()
    {
        var src = new[]
        {
            new SKPoint(0, 0),
            new SKPoint(100, 0),
            new SKPoint(100, 100),
            new SKPoint(0, 100),
        };

        var dst = new[]
        {
            new SKPoint(0, 0),
            new SKPoint(100, 0),
            new SKPoint(100, 100),
            new SKPoint(0, 100),
        };

        var matrix = _computer.ComputePerspectiveMatrix(src, dst);

        var mappedPoint = matrix.MapPoint(src[0]);
        mappedPoint.X.Should().BeApproximately(dst[0].X, 0.1f);
        mappedPoint.Y.Should().BeApproximately(dst[0].Y, 0.1f);

        mappedPoint = matrix.MapPoint(src[2]);
        mappedPoint.X.Should().BeApproximately(dst[2].X, 0.1f);
        mappedPoint.Y.Should().BeApproximately(dst[2].Y, 0.1f);
    }

    [Fact]
    public void ComputePerspectiveMatrix_ShouldHandleTranslation()
    {
        var src = new[]
        {
            new SKPoint(0, 0),
            new SKPoint(10, 0),
            new SKPoint(10, 10),
            new SKPoint(0, 10),
        };

        var offset = 10;
        var dst = new[]
        {
            new SKPoint(0 + offset, 0 + offset),
            new SKPoint(10 + offset, 0 + offset),
            new SKPoint(10 + offset, 10 + offset),
            new SKPoint(0 + offset, 10 + offset),
        };

        var matrix = _computer.ComputePerspectiveMatrix(src, dst);

        var mappedPoint = matrix.MapPoint(src[0]);
        mappedPoint.X.Should().BeApproximately(dst[0].X, 0.1f);
        mappedPoint.Y.Should().BeApproximately(dst[0].Y, 0.1f);
    }
}
