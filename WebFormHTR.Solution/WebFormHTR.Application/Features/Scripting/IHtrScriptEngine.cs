using WebFormHTR.Application.Features.Scripting.DTOs;

namespace WebFormHTR.Application.Features.Scripting;

public interface IHtrScriptEngine
{
    Task<SelectRoisOutputDto> SelectRoisAsync(SelectRoisInputDto input, CancellationToken ct);
    Task<string> AnnotateRoisAsync(Guid executionId, CancellationToken ct);
    Task ManualAlignAsync(Guid templateId, CancellationToken ct);
    Task<ProcessLogsheetOutputDto> ProcessLogsheetAsync(ProcessLogsheetInputDto input, CancellationToken ct);
}