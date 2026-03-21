using FluentAssertions;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Domain.ValueObjects;
using LogsheetXtractor.Infrastructure.Services;
using LogsheetXtractor.Infrastructure.Services.Coordinates;
using Microsoft.Extensions.Logging;
using Moq;
using SkiaSharp;

namespace LogsheetXtractor.Tests.Infrastructure.Services;

public class CoordinateTransformerServiceLoggingTests
{
    private readonly Mock<IPerspectiveMatrixComputer> _perspectiveMatrixComputerMock = new();
    private readonly Mock<ILogger<CoordinateTransformerService>> _loggerMock = new();
    private readonly CoordinateTransformerService _sut;

    public CoordinateTransformerServiceLoggingTests()
    {
        _sut = new CoordinateTransformerService(
            _perspectiveMatrixComputerMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public void TransformCoordinates_ShouldLogDebug_WhenAlignmentPointsAreValid()
    {
        // Arrange
        var destinationCoordinates = new Coordinates(0, 0, 100, 100);
        var sourceCoordinates = new Coordinates(0, 0, 50, 50);
        var alignmentPoints = new List<PointCoordinate>
        {
            new(0, 0),
            new(50, 0),
            new(50, 50),
            new(0, 50),
        };

        _perspectiveMatrixComputerMock
            .Setup(x => x.ComputePerspectiveMatrix(It.IsAny<SKPoint[]>(), It.IsAny<SKPoint[]>()))
            .Returns(SKMatrix.Identity);

        // Act
        _sut.TransformCoordinates(destinationCoordinates, sourceCoordinates, alignmentPoints, 1.0);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) =>
                            v.ToString()!
                                .Contains("Transforming coordinates with perspective matrix")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public void TransformCoordinates_ShouldLogWarning_WhenAlignmentPointsAreInvalid()
    {
        // Arrange
        var destinationCoordinates = new Coordinates(0, 0, 100, 100);
        var sourceCoordinates = new Coordinates(0, 0, 50, 50);
        var alignmentPoints = new List<PointCoordinate>(); // Invalid count

        // Act
        _sut.TransformCoordinates(destinationCoordinates, sourceCoordinates, alignmentPoints, 1.0);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) => v.ToString()!.Contains("Adjustment points invalid or missing")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }
}
