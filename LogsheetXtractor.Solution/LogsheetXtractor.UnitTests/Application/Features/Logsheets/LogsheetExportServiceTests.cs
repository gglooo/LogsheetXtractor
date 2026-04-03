using System.IO.Compression;
using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Features.Logsheets.Export;
using LogsheetXtractor.Application.Features.Scripting;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Domain.ValueObjects;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using MapsterMapper;
using Moq;

namespace LogsheetXtractor.UnitTests.Application.Features.Logsheets;

public class LogsheetExportServiceTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IHtrScriptEngine> _scriptEngineMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly LogsheetExportService _service;

    public LogsheetExportServiceTests()
    {
        _mapperMock
            .Setup(x => x.Map<ExportCoordinateDto>(It.IsAny<Coordinates>()))
            .Returns((Coordinates coordinates) => new ExportCoordinateDto
            {
                X = coordinates.X,
                Y = coordinates.Y,
                Width = coordinates.Width,
                Height = coordinates.Height,
            });

        _service = new LogsheetExportService(
            _dbContext,
            _scriptEngineMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task ExportLogsheetDataAsync_ShouldFail_WhenLogsheetDoesNotExist()
    {
        var result = await _service.ExportLogsheetDataAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("not available for export"));
        _scriptEngineMock.Verify(
            x =>
                x.ExportLogsheetDataAsync(
                    It.IsAny<Logsheet>(),
                    It.IsAny<IEnumerable<ExportLogsheetDataDto>>(),
                    It.IsAny<LogsheetXtractor.Domain.Entities.File>(),
                    It.IsAny<LogsheetXtractor.Domain.Entities.File>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task ExportLogsheetDataAsync_ShouldFail_WhenLogsheetIsNotCompleted()
    {
        var logsheet = CreateLogsheet(ELogSheetStatus.NeedsReview);
        await _dbContext.SaveChangesAsync();

        var result = await _service.ExportLogsheetDataAsync(logsheet.Id, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("not available for export"));
        _scriptEngineMock.Verify(
            x =>
                x.ExportLogsheetDataAsync(
                    It.IsAny<Logsheet>(),
                    It.IsAny<IEnumerable<ExportLogsheetDataDto>>(),
                    It.IsAny<LogsheetXtractor.Domain.Entities.File>(),
                    It.IsAny<LogsheetXtractor.Domain.Entities.File>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task ExportLogsheetDataAsync_ShouldSucceed_WhenLogsheetIsCompleted()
    {
        var logsheet = CreateLogsheet(ELogSheetStatus.Completed);
        await _dbContext.SaveChangesAsync();

        _scriptEngineMock
            .Setup(x =>
                x.ExportLogsheetDataAsync(
                    It.IsAny<Logsheet>(),
                    It.IsAny<IEnumerable<ExportLogsheetDataDto>>(),
                    It.IsAny<LogsheetXtractor.Domain.Entities.File>(),
                    It.IsAny<LogsheetXtractor.Domain.Entities.File>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Result.Ok(
                    new GetFileDto
                    {
                        FileName = "single.xlsx",
                        ContentType =
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        Stream = new MemoryStream([1, 2, 3]),
                    }
                )
            );

        var result = await _service.ExportLogsheetDataAsync(logsheet.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.FileName.Should().Be("single.xlsx");
        _scriptEngineMock.Verify(
            x =>
                x.ExportLogsheetDataAsync(
                    It.Is<Logsheet>(l => l.Id == logsheet.Id),
                    It.IsAny<IEnumerable<ExportLogsheetDataDto>>(),
                    It.IsAny<LogsheetXtractor.Domain.Entities.File>(),
                    It.IsAny<LogsheetXtractor.Domain.Entities.File>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task ExportBatchLogsheetDataAsync_ShouldExportOnlyCompletedLogsheets()
    {
        var completedLogsheet = CreateLogsheet(ELogSheetStatus.Completed);
        var needsReviewLogsheet = CreateLogsheet(ELogSheetStatus.NeedsReview);
        await _dbContext.SaveChangesAsync();

        _scriptEngineMock
            .Setup(x =>
                x.ExportLogsheetDataAsync(
                    It.IsAny<Logsheet>(),
                    It.IsAny<IEnumerable<ExportLogsheetDataDto>>(),
                    It.IsAny<LogsheetXtractor.Domain.Entities.File>(),
                    It.IsAny<LogsheetXtractor.Domain.Entities.File>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                (
                    Logsheet logsheet,
                    IEnumerable<ExportLogsheetDataDto> _,
                    LogsheetXtractor.Domain.Entities.File _,
                    LogsheetXtractor.Domain.Entities.File _,
                    CancellationToken _
                ) =>
                    Result.Ok(
                        new GetFileDto
                        {
                            FileName = $"{logsheet.Id}.xlsx",
                            ContentType =
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            Stream = new MemoryStream([1, 2, 3]),
                        }
                    )
            );

        var result = await _service.ExportBatchLogsheetDataAsync(
            [completedLogsheet.Id, needsReviewLogsheet.Id],
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();

        result.Value.Stream.Should().NotBeNull();
        using var archive = new ZipArchive(
            result.Value.Stream!,
            ZipArchiveMode.Read,
            leaveOpen: false
        );
        archive.Entries.Should().HaveCount(1);
        archive.Entries.Single().Name.Should().Be($"{completedLogsheet.Id}.xlsx");

        _scriptEngineMock.Verify(
            x =>
                x.ExportLogsheetDataAsync(
                    It.Is<Logsheet>(l => l.Id == completedLogsheet.Id),
                    It.IsAny<IEnumerable<ExportLogsheetDataDto>>(),
                    It.IsAny<LogsheetXtractor.Domain.Entities.File>(),
                    It.IsAny<LogsheetXtractor.Domain.Entities.File>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task ExportBatchLogsheetDataAsync_ShouldSucceed_WhenAtLeastOneExportableLogsheetExportsSuccessfully()
    {
        var failingCompletedLogsheet = CreateLogsheet(ELogSheetStatus.Completed);
        var successfulCompletedLogsheet = CreateLogsheet(ELogSheetStatus.Completed);
        await _dbContext.SaveChangesAsync();

        _scriptEngineMock
            .Setup(x =>
                x.ExportLogsheetDataAsync(
                    It.IsAny<Logsheet>(),
                    It.IsAny<IEnumerable<ExportLogsheetDataDto>>(),
                    It.IsAny<LogsheetXtractor.Domain.Entities.File>(),
                    It.IsAny<LogsheetXtractor.Domain.Entities.File>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                (
                    Logsheet logsheet,
                    IEnumerable<ExportLogsheetDataDto> _,
                    LogsheetXtractor.Domain.Entities.File _,
                    LogsheetXtractor.Domain.Entities.File _,
                    CancellationToken _
                ) =>
                {
                    if (logsheet.Id == failingCompletedLogsheet.Id)
                    {
                        return Result.Fail<GetFileDto>("script failed");
                    }

                    return Result.Ok(
                        new GetFileDto
                        {
                            FileName = $"{logsheet.Id}.xlsx",
                            ContentType =
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            Stream = new MemoryStream([5, 6, 7]),
                        }
                    );
                }
            );

        var result = await _service.ExportBatchLogsheetDataAsync(
            [failingCompletedLogsheet.Id, successfulCompletedLogsheet.Id],
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Stream.Should().NotBeNull();
        using var archive = new ZipArchive(
            result.Value.Stream!,
            ZipArchiveMode.Read,
            leaveOpen: false
        );
        archive.Entries.Should().HaveCount(1);
        archive.Entries.Single().Name.Should().Be($"{successfulCompletedLogsheet.Id}.xlsx");
    }

    [Fact]
    public async Task ExportBatchLogsheetDataAsync_ShouldFail_WhenAllExportableLogsheetsFailToExport()
    {
        var completedLogsheet = CreateLogsheet(ELogSheetStatus.Completed);
        await _dbContext.SaveChangesAsync();

        _scriptEngineMock
            .Setup(x =>
                x.ExportLogsheetDataAsync(
                    It.IsAny<Logsheet>(),
                    It.IsAny<IEnumerable<ExportLogsheetDataDto>>(),
                    It.IsAny<LogsheetXtractor.Domain.Entities.File>(),
                    It.IsAny<LogsheetXtractor.Domain.Entities.File>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Fail<GetFileDto>("script failed"));

        var result = await _service.ExportBatchLogsheetDataAsync(
            [completedLogsheet.Id],
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "No logsheets available for export");
    }

    [Fact]
    public async Task ExportBatchLogsheetDataAsync_ShouldFail_WhenNoLogsheetsAreAvailableForExport()
    {
        var needsReviewLogsheet = CreateLogsheet(ELogSheetStatus.NeedsReview);
        await _dbContext.SaveChangesAsync();

        var result = await _service.ExportBatchLogsheetDataAsync(
            [needsReviewLogsheet.Id, Guid.NewGuid()],
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "No logsheets available for export");
        _scriptEngineMock.Verify(
            x =>
                x.ExportLogsheetDataAsync(
                    It.IsAny<Logsheet>(),
                    It.IsAny<IEnumerable<ExportLogsheetDataDto>>(),
                    It.IsAny<LogsheetXtractor.Domain.Entities.File>(),
                    It.IsAny<LogsheetXtractor.Domain.Entities.File>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    private Logsheet CreateLogsheet(ELogSheetStatus status)
    {
        var templateFile = new LogsheetXtractor.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "template.pdf",
            StoredFileName = "template.pdf",
            StoragePath = "/tmp/template.pdf",
        };
        var logsheetFile = new LogsheetXtractor.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "logsheet.pdf",
            StoredFileName = "logsheet.pdf",
            StoragePath = "/tmp/logsheet.pdf",
        };

        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = Guid.NewGuid(),
            Name = $"template-{Guid.NewGuid()}",
            FileId = templateFile.Id,
            File = templateFile,
            Width = 100,
            Height = 100,
        };
        var roi = new Roi
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            Template = template,
            VariableName = "field_1",
            Type = ERoiType.Handwritten,
            Coordinates = new Coordinates(1, 2, 3, 4),
        };
        template.Rois.Add(roi);

        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            Template = template,
            FileId = logsheetFile.Id,
            File = logsheetFile,
            Status = status,
        };

        var extractedValue = new ExtractedValue
        {
            Id = Guid.NewGuid(),
            LogsheetId = logsheet.Id,
            Logsheet = logsheet,
            RoiId = roi.Id,
            Roi = roi,
            Value = "42",
        };
        logsheet.ExtractedValues.Add(extractedValue);

        _dbContext.Logsheets.Add(logsheet);
        return logsheet;
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
