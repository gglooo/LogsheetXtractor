using FluentResults;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Domain.Entities;
using File = WebFormHTR.Domain.Entities.File;

namespace WebFormHTR.IntegrationTests.Infrastructure.Fakes;

public sealed class FakeHtrScriptEngine : IHtrScriptEngine
{
    public Task<Result<SelectRoisOutputDto>> SelectRoisAsync(SelectRoisInputDto input, CancellationToken ct)
    {
        return Task.FromResult(Result.Ok(new SelectRoisOutputDto([], [])));
    }

    public Task<Result<LogsheetDetailDto>> AutomaticAlignAsync(AutomaticAlignmentInputDto input, CancellationToken ct)
    {
        return Task.FromResult(Result.Fail<LogsheetDetailDto>("Not implemented in fake"));
    }

    public Task<Result<ProcessLogsheetOutputDto>> ProcessLogsheetAsync(ProcessLogsheetInputDto input, CancellationToken ct)
    {
        var extractedData = input.Logsheet.Template.Rois
            .ToDictionary(roi => roi.Id.ToString(), _ => "42");

        return Task.FromResult(Result.Ok(new ProcessLogsheetOutputDto(extractedData)));
    }

    public Task<Result<PdfDimensionsDto>> GetPdfDimensionsAsync(File file, CancellationToken ct)
    {
        return Task.FromResult(Result.Ok(new PdfDimensionsDto
        {
            Width = 1000,
            Height = 1400,
        }));
    }

    public Task<Result<GetFileDto>> ExportLogsheetDataAsync(
        Logsheet logsheet,
        IEnumerable<ExportLogsheetDataDto> data,
        File logsheetFile,
        File templateFile,
        CancellationToken ct)
    {
        var dto = new GetFileDto
        {
            Stream = new MemoryStream([]),
            ContentType = "application/json",
            FileName = "export.json",
        };
        return Task.FromResult(Result.Ok(dto));
    }
}
