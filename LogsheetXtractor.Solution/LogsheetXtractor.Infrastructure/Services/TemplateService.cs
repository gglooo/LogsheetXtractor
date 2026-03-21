using System.Text.Json;
using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Residuals;
using LogsheetXtractor.Application.Features.ROIs;
using LogsheetXtractor.Application.Features.Scripting;
using LogsheetXtractor.Application.Features.Scripting.DTOs;
using LogsheetXtractor.Application.Features.Template;
using LogsheetXtractor.Application.Features.Template.CreateTemplate;
using LogsheetXtractor.Application.Features.Template.DTOs;
using LogsheetXtractor.Application.Features.Template.Interfaces;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Infrastructure.Services.Scripting.DTOs;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LogsheetXtractor.Infrastructure.Services;

public class TemplateService(
    IAppDbContext dbContext,
    IMapper mapper,
    IResidualService residualService,
    IRoiService roiService,
    IHtrScriptEngine scriptEngine,
    ILogger<TemplateService> logger
) : ITemplateService
{
    public async Task<Result<TemplateDetailDto>> AddBacksideTemplateAsync(
        Guid templateId,
        string name,
        Guid fileId,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation("Adding backside template for template {TemplateId}", templateId);

        var template = await dbContext.Templates.FirstAsync(
            t => t.Id == templateId,
            cancellationToken
        );
        var backsideParentId = await GetBacksideParentIdAsync(template.ParentId, cancellationToken);

        var backsideTemplateResult = await GetTemplateAsync(name, fileId, backsideParentId);
        if (backsideTemplateResult.IsFailed)
        {
            return backsideTemplateResult.ToResult();
        }

        var backsideTemplate = backsideTemplateResult.Value;
        await dbContext.Templates.AddAsync(backsideTemplate, cancellationToken);
        template.SetBacksideTemplate(backsideTemplate);

        logger.LogInformation(
            "Backside template {BacksideTemplateId} linked to template {TemplateId}",
            backsideTemplate.Id,
            template.Id
        );
        return Result.Ok(mapper.Map<TemplateDetailDto>(template));
    }

    public async Task<Result<TemplateDetailDto>> CreateTemplateAsync(
        CreateTemplateCommand command,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation("Creating template {TemplateName}", command.Name);

        Template? backsideTemplate = null;
        if (command.Backside is not null)
        {
            var backsideResult = command.Backside.ImportedConfig is not null
                ? await GetTemplateFromImportedConfigAsync(
                    command.Backside.ImportedConfig,
                    command.Backside.FileId,
                    command.Backside.Name,
                    command.Backside.ParentId
                )
                : await GetTemplateAsync(
                    command.Backside.Name,
                    command.Backside.FileId,
                    command.Backside.ParentId
                );

            if (backsideResult.IsFailed)
            {
                return backsideResult.ToResult();
            }

            backsideTemplate = backsideResult.Value;
        }

        var templateResult = command.ImportedConfig is not null
            ? await GetTemplateFromImportedConfigAsync(
                command.ImportedConfig,
                command.FileId,
                command.Name,
                command.ParentId
            )
            : await GetTemplateAsync(command.Name, command.FileId, command.ParentId);

        if (templateResult.IsFailed)
        {
            return templateResult.ToResult();
        }

        var template = templateResult.Value;

        await dbContext.Templates.AddAsync(template, cancellationToken);

        template.SetBacksideTemplate(backsideTemplate);

        logger.LogInformation(
            "Template {TemplateId} ('{TemplateName}') created successfully",
            template.Id,
            template.Name
        );
        return Result.Ok(mapper.Map<TemplateDetailDto>(template));
    }

    public async Task<Result<TemplateDetailDto>> CloneTemplateAsync(
        Guid templateId,
        string newTemplateName,
        Guid fileId,
        CloneTemplateBacksideCommand? backside,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation(
            "Cloning template {TemplateId} to new name '{NewTemplateName}'",
            templateId,
            newTemplateName
        );
        var parentTemplate = await dbContext
            .Templates.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

        if (parentTemplate is null)
        {
            return Result.Fail(new NotFoundError("Parent template not found"));
        }

        var clonedTemplateResult = await GetTemplateAsync(
            newTemplateName,
            fileId,
            parentTemplate.Id
        );
        if (clonedTemplateResult.IsFailed)
        {
            return clonedTemplateResult.ToResult();
        }

        var clonedTemplate = clonedTemplateResult.Value;
        clonedTemplate.Id = Guid.NewGuid();

        await dbContext.Templates.AddAsync(clonedTemplate, cancellationToken);

        if (backside is not null)
        {
            var backsideTemplateResult = await GetTemplateAsync(
                backside.Name,
                backside.FileId,
                null
            );
            if (backsideTemplateResult.IsFailed)
            {
                return backsideTemplateResult.ToResult();
            }

            var backsideTemplate = backsideTemplateResult.Value;
            backsideTemplate.Id = Guid.NewGuid();

            await dbContext.Templates.AddAsync(backsideTemplate, cancellationToken);
            clonedTemplate.SetBacksideTemplate(backsideTemplate);

            if (parentTemplate.BacksideTemplateId.HasValue)
            {
                backsideTemplate.ParentId = parentTemplate.BacksideTemplateId.Value;

                var cloneResBacksideResult = await residualService.CloneResidualsForTemplateAsync(
                    parentTemplate.BacksideTemplateId.Value,
                    backsideTemplate.Id,
                    cancellationToken
                );
                if (cloneResBacksideResult.IsFailed)
                {
                    return cloneResBacksideResult.ToResult();
                }

                var cloneRoiBacksideResult = await roiService.CloneRoisForTemplateAsync(
                    parentTemplate.BacksideTemplateId.Value,
                    backsideTemplate.Id,
                    cancellationToken
                );
                if (cloneRoiBacksideResult.IsFailed)
                {
                    return cloneRoiBacksideResult.ToResult();
                }
            }
        }

        var cloneResResult = await residualService.CloneResidualsForTemplateAsync(
            parentTemplate.Id,
            clonedTemplate.Id,
            cancellationToken
        );
        if (cloneResResult.IsFailed)
        {
            return cloneResResult.ToResult();
        }

        var cloneRoiResult = await roiService.CloneRoisForTemplateAsync(
            parentTemplate.Id,
            clonedTemplate.Id,
            cancellationToken
        );
        if (cloneRoiResult.IsFailed)
        {
            return cloneRoiResult.ToResult();
        }

        logger.LogInformation(
            "Template cloned successfully. New Template ID: {NewTemplateId}",
            clonedTemplate.Id
        );

        return Result.Ok(mapper.Map<TemplateDetailDto>(clonedTemplate));
    }

    public async Task<Result<string>> ExportTemplateConfigAsync(
        Guid templateId,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation("Exporting config for template {TemplateId}", templateId);
        var template = await dbContext
            .Templates.AsNoTracking()
            .Include(t => t.File)
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

        if (template is null)
        {
            logger.LogWarning("Template {TemplateId} not found for export", templateId);
            return Result.Fail(new NotFoundError("Template not found"));
        }

        var templateConfig = mapper.Map<PythonTemplateConfig>(template);
        var options = new JsonSerializerOptions { WriteIndented = true };
        return Result.Ok(JsonSerializer.Serialize(templateConfig, options));
    }

    private async Task<Result<PdfDimensionsDto>> CalculateTemplateFileDimensionsAsync(Guid fileId)
    {
        var file = await dbContext.Files.FirstOrDefaultAsync(f => f.Id == fileId);
        if (file is null)
        {
            logger.LogError("File {FileId} not found", fileId);
            return Result.Fail(new NotFoundError("File not found"));
        }

        try
        {
            var dimensionsResult = await scriptEngine.GetPdfDimensionsAsync(
                file,
                CancellationToken.None
            );
            if (dimensionsResult.IsFailed)
            {
                var errorMessage =
                    dimensionsResult.Errors.FirstOrDefault()?.Message ?? "Unknown error";
                logger.LogError(
                    "Unable to get PDF dimensions for file {FileId}: {Error}",
                    fileId,
                    errorMessage
                );
                return Result.Fail(new InvalidStateError("Unable to get PDF dimensions"));
            }

            return Result.Ok(dimensionsResult.Value);
        }
        catch (Exception ex)
        {
            return Result.Fail(
                new InvalidStateError($"Error calculating template dimensions: {ex.Message}")
            );
        }
    }

    private async Task<Result<Template>> GetTemplateAsync(
        string templateName,
        Guid fileId,
        Guid? parentId
    )
    {
        var dimensionsResult = await CalculateTemplateFileDimensionsAsync(fileId);
        if (dimensionsResult.IsFailed)
        {
            return dimensionsResult.ToResult();
        }

        return Result.Ok(
            new Template
            {
                Name = templateName,
                ParentId = parentId,
                FileId = fileId,
                Width = dimensionsResult.Value.Width,
                Height = dimensionsResult.Value.Height,
            }
        );
    }

    private async Task<Result<Template>> GetTemplateFromImportedConfigAsync(
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

    private async Task<Guid?> GetBacksideParentIdAsync(
        Guid? templateParentId,
        CancellationToken cancellationToken
    )
    {
        if (!templateParentId.HasValue)
        {
            return null;
        }

        return await dbContext
            .Templates.Where(t => t.Id == templateParentId.Value)
            .Select(t => t.BacksideTemplateId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
