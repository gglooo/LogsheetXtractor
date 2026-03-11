using FluentAssertions;
using Moq;
using SkiaSharp;
using WebFormHTR.Domain.ValueObjects;
using WebFormHTR.Infrastructure.Services.Coordinates;
using WebFormHTR.Application.Interfaces;
using Microsoft.Extensions.Logging;
using WebFormHTR.Infrastructure.Services;
using Xunit;

namespace WebFormHTR.Tests.Infrastructure.Services;

public class CoordinateTransformerServiceTests
{
    private readonly Mock<IPerspectiveMatrixComputer> _perspectiveMatrixComputerMock = new();
    private readonly Mock<ILogger<CoordinateTransformerService>> _loggerMock = new();
    private readonly CoordinateTransformerService _sut;

    public CoordinateTransformerServiceTests()
    {
        _sut = new CoordinateTransformerService(_perspectiveMatrixComputerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void NormalizeAlignmentPoints_ShouldScaleCoordinatesToTemplateSpace()
    {
        // Arrange
        var rawTemplatePoints = new List<PointCoordinate>();
        var rawTargetPoints = new List<PointCoordinate>();

        var templateWidth = 500;
        var templateHeight = 1000;
        var imageWidth = 2500;
        var imageHeight = 5000;

        // Mock compute to return Identity, which means corners map perfectly to themselves
        _perspectiveMatrixComputerMock
            .Setup(x => x.ComputePerspectiveMatrix(It.IsAny<SKPoint[]>(), It.IsAny<SKPoint[]>()))
            .Returns(SKMatrix.Identity);

        // Act
        var result = _sut.NormalizeAlignmentPoints(
            rawTemplatePoints, rawTargetPoints, templateWidth, templateHeight, imageWidth, imageHeight);

        // Assert
        result.Should().HaveCount(4);
        result[0].Should().BeEquivalentTo(new PointCoordinate(0, 0));
        result[1].Should().BeEquivalentTo(new PointCoordinate(500, 0));
        result[2].Should().BeEquivalentTo(new PointCoordinate(500, 1000));
        result[3].Should().BeEquivalentTo(new PointCoordinate(0, 1000));
    }
}