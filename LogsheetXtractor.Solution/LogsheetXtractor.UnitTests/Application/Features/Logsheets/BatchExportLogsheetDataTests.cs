using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Features.Logsheets;
using LogsheetXtractor.Application.Features.Logsheets.Export;
using Moq;
using Xunit;

namespace LogsheetXtractor.UnitTests.Application.Features.Logsheets;

public class BatchExportLogsheetDataTests
{
    private readonly Mock<ILogsheetExportService> _logsheetExportServiceMock = new();

    [Fact]
    public async Task Handle_ShouldReturnFile_WhenExportSucceeds()
    {
        var logsheetIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var command = new BatchExportLogsheetDataCommand(logsheetIds);
        var expectedFile = new GetFileDto
        {
            Stream = new MemoryStream(),
            FileName = "batch_export.zip",
            ContentType = "application/zip",
        };

        _logsheetExportServiceMock
            .Setup(x => x.ExportBatchLogsheetDataAsync(logsheetIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(expectedFile));

        var result = await BatchExportLogsheetDataHandler.Handle(
            command,
            _logsheetExportServiceMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedFile);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenExportThrowsException()
    {
        var logsheetIds = new[] { Guid.NewGuid() };
        var command = new BatchExportLogsheetDataCommand(logsheetIds);
        var exceptionMessage = "Batch export failed";

        _logsheetExportServiceMock
            .Setup(x => x.ExportBatchLogsheetDataAsync(logsheetIds, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var result = await BatchExportLogsheetDataHandler.Handle(
            command,
            _logsheetExportServiceMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains(exceptionMessage));
    }
}
