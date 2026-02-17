using System.Text.Json;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Infrastructure.Services.Scripting.DTOs;
using WebFormHTR.Infrastructure.Services.Storage;

namespace WebFormHTR.Infrastructure.Services.Scripting;

public class ScriptOutputParser(IFileStorageService fileStorageService) : IScriptOutputParser
{
    public async Task<Dictionary<string, string>> ParseProcessLogsheetCsvAsync(string filePath, CancellationToken ct = default)
    {
        var csvContent = await fileStorageService.ReadAllTextAsync(filePath, ct);
        var result = new Dictionary<string, string>();

        var lines = csvContent.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Skip(1);
        foreach (var line in lines)
        {
            var parts = line.Split(',', 2);
            if (parts.Length == 2)
            {
                result[parts[0]] = parts[1];
            }
        }

        return result;
    }

    public async Task<SelectRoisOutputDto> ParseSelectRoisJsonAsync(string filePath, Guid templateId, CancellationToken ct = default)
    {
        var jsonContent = await fileStorageService.ReadAllTextAsync(filePath, ct);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var rois = JsonSerializer.Deserialize<PythonSelectRoisOutputDto>(jsonContent, options);

        return rois?.ToSelectRoisOutputDtoList(templateId) ?? new SelectRoisOutputDto([], []);
    }
}