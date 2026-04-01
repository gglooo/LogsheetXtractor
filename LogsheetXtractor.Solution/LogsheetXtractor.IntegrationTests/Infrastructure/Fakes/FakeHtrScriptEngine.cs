using FluentResults;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Features.File.DTOs;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Features.Scripting;
using LogsheetXtractor.Application.Features.Scripting.DTOs;
using LogsheetXtractor.Domain.Entities;
using File = LogsheetXtractor.Domain.Entities.File;

namespace LogsheetXtractor.IntegrationTests.Infrastructure.Fakes;

public sealed class FakeHtrScriptEngine : IHtrScriptEngine
{
    public IReadOnlyList<ExportLogsheetDataDto> LastExportData { get; private set; } = [];

    public void ResetCapturedExportData()
    {
        LastExportData = [];
    }

    public Task<Result<SelectRoisOutputDto>> SelectRoisAsync(
        SelectRoisInputDto input,
        CancellationToken ct
    )
    {
        return Task.FromResult(Result.Ok(new SelectRoisOutputDto([], [])));
    }

    public Task<Result<LogsheetDetailDto>> AutomaticAlignAsync(
        AutomaticAlignmentInputDto input,
        CancellationToken ct
    )
    {
        return Task.FromResult(Result.Fail<LogsheetDetailDto>("Not implemented in fake"));
    }

    public Task<Result<ProcessLogsheetOutputDto>> ProcessLogsheetAsync(
        ProcessLogsheetInputDto input,
        CancellationToken ct
    )
    {
        var extractedData = input.Logsheet.Template.Rois.ToDictionary(
            roi => roi.Id.ToString(),
            _ => "42"
        );

        return Task.FromResult(Result.Ok(new ProcessLogsheetOutputDto(extractedData)));
    }

    public Task<Result<PdfDimensionsDto>> GetPdfDimensionsAsync(File file, CancellationToken ct)
    {
        return Task.FromResult(Result.Ok(new PdfDimensionsDto { Width = 1000, Height = 1400 }));
    }

    public Task<Result<GetFileDto>> ExportLogsheetDataAsync(
        Logsheet logsheet,
        IEnumerable<ExportLogsheetDataDto> data,
        File logsheetFile,
        File templateFile,
        CancellationToken ct
    )
    {
        LastExportData = data
            .Select(
                entry =>
                    new ExportLogsheetDataDto
                    {
                        VariableName = entry.VariableName,
                        Value = entry.Value,
                        Coordinates = entry.Coordinates,
                        Page = entry.Page,
                    }
            )
            .ToList();

        var dto = new GetFileDto
        {
            Stream = new MemoryStream([]),
            ContentType = "application/json",
            FileName = "export.json",
        };
        return Task.FromResult(Result.Ok(dto));
    }
}
