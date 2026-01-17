using WebFormHTR.Application.Features.Scripting.DTOs;

namespace WebFormHTR.Infrastructure.Services.Scripting;

public interface IScriptOutputParser
{
    Dictionary<string, string> ParseProcessLogsheetCsv(string filePath);
    SelectRoisOutputDto ParseSelectRoisJson(string filePath, Guid templateId);
}