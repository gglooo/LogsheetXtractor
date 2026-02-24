using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Infrastructure.Services.Scripting;

public interface IScriptOutputParser
{
    Task<Dictionary<string, string>> ParseProcessLogsheetCsvAsync(string filePath, CancellationToken ct = default);

    Task<SelectRoisOutputDto>
        ParseSelectRoisJsonAsync(string filePath, Guid templateId, CancellationToken ct = default);

    AlignmentContainer ParseAutomaticAlignmentJson(string rawJson, int templateWidth, int templateHeight,
        int? backsideTemplateWidth = null, int? backsideTemplateHeight = null);
}