using System.Text.Json;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.ValueObjects;
using WebFormHTR.Infrastructure.Services.Scripting.DTOs;
using WebFormHTR.Infrastructure.Services.Storage;

namespace WebFormHTR.Infrastructure.Services.Scripting;

public class ScriptOutputParser(
    IFileStorageService fileStorageService,
    ICoordinateTransformerService coordinateTransformerService) : IScriptOutputParser
{
    public async Task<Dictionary<string, string>> ParseProcessLogsheetCsvAsync(string filePath,
        CancellationToken ct = default)
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

    public async Task<SelectRoisOutputDto> ParseSelectRoisJsonAsync(string filePath, Guid templateId,
        CancellationToken ct = default)
    {
        var jsonContent = await fileStorageService.ReadAllTextAsync(filePath, ct);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var rois = JsonSerializer.Deserialize<PythonSelectRoisOutputDto>(jsonContent, options);

        return rois?.ToSelectRoisOutputDtoList(templateId) ?? new SelectRoisOutputDto([], []);
    }

    public AlignmentContainer ParseAutomaticAlignmentJson(string rawJson, int templateWidth, int templateHeight,
        int? backsideTemplateWidth = null, int? backsideTemplateHeight = null)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var rawAlignment = JsonSerializer.Deserialize<PythonAlignmentOutputDto>(rawJson, options);

        if (rawAlignment?.Frontside == null)
        {
            throw new ArgumentException("Invalid alignment JSON format.");
        }

        if (rawAlignment?.Backside != null && (backsideTemplateWidth == null || backsideTemplateHeight == null))
        {
            throw new ArgumentException(
                "Backside template dimensions must be provided if backside alignment is present.");
        }

        var frontAlignmentPoints = coordinateTransformerService.NormalizeAlignmentPoints(
            rawAlignment!.Frontside.TemplatePoints, rawAlignment.Frontside.TargetPoints, templateWidth, templateHeight,
            rawAlignment.Frontside.ImageWidth, rawAlignment.Frontside.ImageHeight);

        var backAlignmentPoints = rawAlignment.Backside != null
            ? coordinateTransformerService.NormalizeAlignmentPoints(
                rawAlignment.Backside.TemplatePoints, rawAlignment.Backside.TargetPoints, backsideTemplateWidth!.Value,
                backsideTemplateHeight!.Value, rawAlignment.Backside.ImageWidth, rawAlignment.Backside.ImageHeight)
            : null;

        return new AlignmentContainer(frontAlignmentPoints, backAlignmentPoints);
    }
}