using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Features.ExtractedValues;
using LogsheetXtractor.Application.Features.File.Interfaces;
using LogsheetXtractor.Application.Features.PdfCropper;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.ValueObjects;
using LogsheetXtractor.IntegrationTests.Common;
using Moq;
using Xunit;

namespace LogsheetXtractor.IntegrationTests.Infrastructure.Services;

public class ExtractedValuesServiceTests
{
    private readonly Mock<IFileService> _fileServiceMock = new();
    private readonly Mock<IPdfCropperService> _pdfCropperServiceMock = new();
    private readonly Mock<ICoordinateTransformerService> _coordinateTransformerMock = new();
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<ExtractedValuesService>> _loggerMock =
        new();
    private readonly ExtractedValuesService _service;

    public ExtractedValuesServiceTests()
    {
        _service = new ExtractedValuesService(
            _fileServiceMock.Object,
            _pdfCropperServiceMock.Object,
            _coordinateTransformerMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GetExtractedValueImageAsync_ShouldReturnImage_WhenFileExists()
    {
        var template = new Template { Width = 100, Height = 100 };
        var logsheet = new Logsheet
        {
            FileId = Guid.NewGuid(),
            Template = template,
            AlignmentData = new AlignmentContainer(new List<PointCoordinate> { new(0, 0) }, null),
        };
        var roi = new Roi { Coordinates = new Coordinates(10, 10, 10, 10) };
        var extractedValue = new ExtractedValue
        {
            Id = Guid.NewGuid(),
            Logsheet = logsheet,
            Roi = roi,
        };

        var fileStream = new MemoryStream();
        var fileDto = new GetFileDto { Stream = fileStream };
        var croppedStream = new MemoryStream();

        _fileServiceMock.Setup(x => x.GetFileAsync(logsheet.FileId)).ReturnsAsync(fileDto);

        _coordinateTransformerMock
            .Setup(x =>
                x.TransformCoordinates(
                    It.IsAny<Coordinates>(),
                    It.IsAny<Coordinates>(),
                    It.IsAny<List<PointCoordinate>>()
                )
            )
            .Returns(roi.Coordinates);

        _pdfCropperServiceMock
            .Setup(x =>
                x.GetCroppedSection(
                    It.IsAny<byte[]>(),
                    0,
                    10,
                    10,
                    10,
                    10,
                    100,
                    100,
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(croppedStream);

        var requestDto = new GetExtractedValueImageDto(
            extractedValue.Id,
            logsheet.FileId,
            roi.Coordinates,
            template.Width.Value,
            template.Height.Value,
            logsheet.AlignmentData!.Frontside
        );
        var result = await _service.GetExtractedValueImageAsync(requestDto, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Stream.Should().BeSameAs(croppedStream);
        result.Value.ContentType.Should().Be("image/png");
    }

    [Fact]
    public async Task GetExtractedValueImageAsync_ShouldFail_WhenFileStreamIsNull()
    {
        var logsheet = new Logsheet { FileId = Guid.NewGuid() };
        var extractedValue = new ExtractedValue { Logsheet = logsheet };

        _fileServiceMock
            .Setup(x => x.GetFileAsync(logsheet.FileId))
            .ReturnsAsync((GetFileDto?)null);

        var requestDto = new GetExtractedValueImageDto(
            extractedValue.Id,
            logsheet.FileId,
            new Coordinates(0, 0, 0, 0),
            100,
            100,
            null
        );
        var result = await _service.GetExtractedValueImageAsync(requestDto, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Logsheet file not found");
    }
}
