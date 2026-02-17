using WebFormHTR.Application.Features.Scripting.DTOs;

namespace WebFormHTR.Infrastructure.Services.Scripting;

public interface IScriptOutputParser
{
    Task<Dictionary<string, string>> ParseProcessLogsheetCsvAsync(string filePath, CancellationToken ct = default);
    Task<SelectRoisOutputDto> ParseSelectRoisJsonAsync(string filePath, Guid templateId, CancellationToken ct = default);
}