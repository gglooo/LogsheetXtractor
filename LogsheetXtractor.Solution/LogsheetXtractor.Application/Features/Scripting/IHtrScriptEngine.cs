using FluentResults;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Features.File.DTOs;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Features.Scripting.DTOs;
using LogsheetXtractor.Domain.Entities;

namespace LogsheetXtractor.Application.Features.Scripting;

public interface IHtrScriptEngine
{
    Task<Result<SelectRoisOutputDto>> SelectRoisAsync(
        SelectRoisInputDto input,
        CancellationToken ct
    );
    Task<Result<LogsheetDetailDto>> AutomaticAlignAsync(
        AutomaticAlignmentInputDto input,
        CancellationToken ct
    );
    Task<Result<ProcessLogsheetOutputDto>> ProcessLogsheetAsync(
        ProcessLogsheetInputDto input,
        CancellationToken ct
    );
    Task<Result<PdfDimensionsDto>> GetPdfDimensionsAsync(
        LogsheetXtractor.Domain.Entities.File file,
        CancellationToken ct
    );

    Task<Result<GetFileDto>> ExportLogsheetDataAsync(
        Logsheet logsheet,
        IEnumerable<ExportLogsheetDataDto> data,
        LogsheetXtractor.Domain.Entities.File logsheetFile,
        LogsheetXtractor.Domain.Entities.File templateFile,
        CancellationToken ct
    );
}
