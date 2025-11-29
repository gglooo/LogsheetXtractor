using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Infrastructure.Services;

public class PythonHtrAdapter(IScriptExecutor scriptExecutor) : IHtrScriptEngine
{
    public Task<SelectRoisOutputDto> SelectRoisAsync(SelectRoisInputDto input, CancellationToken ct)
    {
        scriptExecutor.ExecuteScriptAsync("select_rois.py", "{'a': 12}", ct);

        throw new NotImplementedException();
    }

    public Task<string> AnnotateRoisAsync(Guid executionId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task ManualAlignAsync(Guid templateId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<ProcessLogsheetOutputDto> ProcessLogsheetAsync(ProcessLogsheetInputDto input, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}