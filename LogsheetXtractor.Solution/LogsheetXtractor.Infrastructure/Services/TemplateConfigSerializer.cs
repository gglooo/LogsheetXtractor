using System.Text.Json;
using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Template.Interfaces;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Infrastructure.Services.Scripting.DTOs;
using MapsterMapper;
using Microsoft.Extensions.Logging;

namespace LogsheetXtractor.Infrastructure.Services;

public class TemplateConfigSerializer(
    IMapper mapper,
    ILogger<TemplateConfigSerializer> logger
) : ITemplateConfigSerializer
{
    public Result<Template> DeserializeTemplateConfig(
        string importedConfig,
        Guid fileId,
        string templateName,
        Guid? parentId
    )
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var deserialized = JsonSerializer.Deserialize<PythonTemplateConfig>(
                importedConfig,
                options
            );

            if (deserialized is null)
            {
                logger.LogError("Imported configuration deserialized to null");
                return Result.Fail(new ValidationError("Imported configuration is null"));
            }

            var template = mapper.Map<Template>(deserialized);
            template.FileId = fileId;
            template.Name = templateName;
            template.ParentId = parentId;

            return Result.Ok(template);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Invalid imported configuration format");
            return Result.Fail(new ValidationError("Invalid imported configuration format"));
        }
    }

    public Result<string> SerializeTemplateConfig(
        Template template,
        bool includeRoiValidations
    )
    {
        var templateConfig = mapper.Map<PythonTemplateConfig>(template);
        var rois = templateConfig.Rois.ToList();
        var variableNamesByRoiId = template.Rois.ToDictionary(r => r.Id, r => r.VariableName);

        foreach (var roi in rois)
        {
            if (
                Guid.TryParse(roi.VarName, out var roiId)
                && variableNamesByRoiId.TryGetValue(roiId, out var variableName)
            )
            {
                // The app uses the unique ROI id as a variable name when interacting with formHTR.
                // This ensures that the user sees the custom variable names when exporting.
                roi.VarName = variableName;
            }

            if (!includeRoiValidations)
            {
                roi.ValidationCondition = null;
            }
        }

        templateConfig.Rois = rois;

        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        return Result.Ok(JsonSerializer.Serialize(templateConfig, jsonOptions));
    }
}
