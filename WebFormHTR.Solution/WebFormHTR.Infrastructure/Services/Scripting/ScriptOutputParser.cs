using System.Text.Json;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Infrastructure.Services.Scripting.DTOs;
using WebFormHTR.Infrastructure.Services.Storage;

namespace WebFormHTR.Infrastructure.Services.Scripting;

public class ScriptOutputParser(IFileStorageService fileStorageService) : IScriptOutputParser
{
    public Dictionary<string, (string, bool)> ParseProcessLogsheetCsv(string filePath)
    {
        var csvContent = fileStorageService.ReadAllText(filePath);
        var result = new Dictionary<string, (string, bool)>();

        var lines = csvContent.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Skip(1);
        foreach (var line in lines)
        {
            var parts = line.Split(',', 3);
            if (parts.Length == 3)
            {
                var varName = parts[0];
                result[varName] = (parts[1], parts[2].Trim().ToLower() == "true");
            }
        }

        return result;
    }

    public SelectRoisOutputDto ParseSelectRoisJson(string filePath, Guid templateId)
    {
        var jsonContent = fileStorageService.ReadAllText(filePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var rois = JsonSerializer.Deserialize<PythonSelectRoisOutputDto>(jsonContent, options);

        return rois?.ToSelectRoisOutputDtoList(templateId) ?? new SelectRoisOutputDto([], []);
    }
}