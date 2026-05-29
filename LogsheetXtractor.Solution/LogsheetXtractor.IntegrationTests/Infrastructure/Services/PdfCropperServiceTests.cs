using Docnet.Core;
using Docnet.Core.Exceptions;
using Docnet.Core.Models;
using Docnet.Core.Readers;
using FluentAssertions;
using LogsheetXtractor.Application.Features.Scripting.DTOs;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Infrastructure.Services;
using Moq;
using SkiaSharp;
using Xunit;

namespace LogsheetXtractor.IntegrationTests.Infrastructure.Services;

public class PdfCropperServiceTests
{
    private readonly Mock<IDocLib> _docLibMock = new();
    private readonly Mock<IPerspectiveMatrixComputer> _matrixComputerMock = new();
    private readonly Mock<IDocReader> _docReaderMock = new();
    private readonly Mock<IPageReader> _pageReaderMock = new();
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<PdfCropperService>> _loggerMock =
        new();
    private readonly PdfCropperService _service;

    public PdfCropperServiceTests()
    {
        _service = new PdfCropperService(
            _docLibMock.Object,
            _matrixComputerMock.Object,
            _loggerMock.Object
        );

        _docLibMock
            .Setup(x => x.GetDocReader(It.IsAny<byte[]>(), It.IsAny<PageDimensions>()))
            .Returns(_docReaderMock.Object);
        _docReaderMock.Setup(x => x.GetPageReader(It.IsAny<int>())).Returns(_pageReaderMock.Object);
    }

    [Fact]
    public void GetPageDimensions_ShouldReturnCorrectDimensions()
    {
        int width = 100;
        int height = 200;

        _pageReaderMock.Setup(x => x.GetPageWidth()).Returns(width);
        _pageReaderMock.Setup(x => x.GetPageHeight()).Returns(height);

        var result = _service.GetPageDimensions(new byte[0], 0, CancellationToken.None);

        result.Width.Should().Be(width);
        result.Height.Should().Be(height);
    }

    [Fact]
    public void GetPageDimensions_ShouldThrow_WhenDocLibFails()
    {
        _docLibMock
            .Setup(x => x.GetDocReader(It.IsAny<byte[]>(), It.IsAny<PageDimensions>()))
            .Throws(new DocnetException("Failed"));

        Action act = () => _service.GetPageDimensions(new byte[0], 0, CancellationToken.None);

        act.Should().Throw<DocnetException>();
    }

    [Fact]
    public void GetPageCount_ShouldReturnPageCount()
    {
        int expectedCount = 5;
        _docReaderMock.Setup(x => x.GetPageCount()).Returns(expectedCount);

        var result = _service.GetPageCount(new byte[0], CancellationToken.None);

        result.Should().Be(expectedCount);
    }
}
