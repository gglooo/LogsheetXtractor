using FluentAssertions;
using FluentResults;
using Moq;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Features.File.Interfaces;
using WebFormHTR.Application.Features.PdfCropper;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.ValueObjects;
using WebFormHTR.Infrastructure.Extensions;
using WebFormHTR.Infrastructure.Services;
using Xunit;

namespace WebFormHTR.Tests.Infrastructure.Services;

public class ExtractedValuesServiceTests
{
    private readonly Mock<IFileService> _fileServiceMock = new();
    private readonly Mock<IPdfCropperService> _pdfCropperServiceMock = new();
    private readonly Mock<ICoordinateTransformerService> _coordinateTransformerMock = new();
    private readonly ExtractedValuesService _service;

    public ExtractedValuesServiceTests()
    {
        _service = new ExtractedValuesService(
            _fileServiceMock.Object,
            _pdfCropperServiceMock.Object,
            _coordinateTransformerMock.Object);
    }

    [Fact]
    public async Task GetExtractedValueImageAsync_ShouldReturnImage_WhenFileExists()
    {
        var template = new Domain.Entities.Template { Width = 100, Height = 100 };
        var logsheet = new Logsheet 
        { 
            FileId = Guid.NewGuid(), 
            Template = template,
            AlignmentDataModelConfig = new AlignmentContainer { Frontside = new List<PointCoordinate> { new PointCoordinate { X = 0, Y = 0 } } }
        };
        var roi = new Roi { Coordinates = new Coordinates { X = 10, Y = 10, Width = 10, Height = 10 } };
        var extractedValue = new ExtractedValue { Id = Guid.NewGuid(), Logsheet = logsheet, Roi = roi };
        
        var fileStream = new MemoryStream();
        var fileDto = new GetFileDto { Stream = fileStream };
        var croppedStream = new MemoryStream();

        _fileServiceMock.Setup(x => x.GetFileAsync(logsheet.FileId))
            .ReturnsAsync(fileDto);

        _coordinateTransformerMock.Setup(x => x.TransformCoordinates(It.IsAny<Coordinates>(), It.IsAny<Coordinates>(), It.IsAny<List<PointCoordinate>>()))
            .Returns(roi.Coordinates);

        _pdfCropperServiceMock.Setup(x => x.GetCroppedSection(It.IsAny<byte[]>(), 0, 10, 10, 10, 10, 100, 100, It.IsAny<CancellationToken>()))
            .Returns(croppedStream);

        var result = await _service.GetExtractedValueImageAsync(extractedValue, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Stream.Should().BeSameAs(croppedStream);
        result.Value.ContentType.Should().Be("image/png");
    }

    [Fact]
    public async Task GetExtractedValueImageAsync_ShouldFail_WhenFileStreamIsNull()
    {
        var logsheet = new Logsheet { FileId = Guid.NewGuid() };
        var extractedValue = new ExtractedValue { Logsheet = logsheet };

        _fileServiceMock.Setup(x => x.GetFileAsync(logsheet.FileId))
            .ReturnsAsync((GetFileDto?)null);

        var result = await _service.GetExtractedValueImageAsync(extractedValue, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Logsheet file not found");
    }
}
