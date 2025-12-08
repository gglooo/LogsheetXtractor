using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Features.Scripting.DTOs;

namespace WebFormHTR.Application.Features.Scripting;

public interface IHtrScriptEngine
{
    Task<SelectRoisOutputDto> SelectRoisAsync(SelectRoisInputDto input, CancellationToken ct);
    Task<string> AnnotateRoisAsync(Guid executionId, CancellationToken ct);
    Task<LogsheetDetailDto> AutomaticAlignAsync(AutomaticAlignmentInputDto input, CancellationToken ct);
    Task<ProcessLogsheetOutputDto> ProcessLogsheetAsync(ProcessLogsheetInputDto input, CancellationToken ct);
    Task<PdfDimensionsDto> GetPdfDimensionsAsync(Domain.Entities.File file, CancellationToken ct);
}