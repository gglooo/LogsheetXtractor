using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Features.Logsheets;
using LogsheetXtractor.Application.Features.Logsheets.Export;
using Moq;
using Xunit;

namespace LogsheetXtractor.Tests.Application.Features.Logsheets;

public class ExportLogsheetDataTests
{
    private readonly Mock<ILogsheetExportService> _logsheetExportServiceMock = new();

    [Fact]
    public async Task Handle_ShouldReturnFile_WhenExportSucceeds()
    {
        var logsheetId = Guid.NewGuid();
        var command = new ExportLogsheetDataCommand(logsheetId);
        var expectedFile = new GetFileDto
        {
            Stream = new MemoryStream(),
            FileName = "export.xlsx",
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        };

        _logsheetExportServiceMock
            .Setup(x => x.ExportLogsheetDataAsync(logsheetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(expectedFile));

        var result = await ExportLogsheetDataHandler.Handle(
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
        var logsheetId = Guid.NewGuid();
        var command = new ExportLogsheetDataCommand(logsheetId);
        var exceptionMessage = "Export failed";

        _logsheetExportServiceMock
            .Setup(x => x.ExportLogsheetDataAsync(logsheetId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var result = await ExportLogsheetDataHandler.Handle(
            command,
            _logsheetExportServiceMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains(exceptionMessage));
    }
}
