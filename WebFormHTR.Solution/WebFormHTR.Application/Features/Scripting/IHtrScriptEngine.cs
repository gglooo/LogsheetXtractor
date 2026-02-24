using FluentResults;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Domain.Entities;

namespace WebFormHTR.Application.Features.Scripting;

public interface IHtrScriptEngine
{
    Task<Result<SelectRoisOutputDto>> SelectRoisAsync(SelectRoisInputDto input, CancellationToken ct);
    Task<Result<LogsheetDetailDto>> AutomaticAlignAsync(AutomaticAlignmentInputDto input, CancellationToken ct);
    Task<Result<ProcessLogsheetOutputDto>> ProcessLogsheetAsync(ProcessLogsheetInputDto input, CancellationToken ct);
    Task<Result<PdfDimensionsDto>> GetPdfDimensionsAsync(Domain.Entities.File file, CancellationToken ct);

    Task<Result<GetFileDto>> ExportLogsheetDataAsync(Logsheet logsheet, IEnumerable<ExportLogsheetDataDto> data,
        Domain.Entities.File logsheetFile,
        Domain.Entities.File templateFile, CancellationToken ct);
}