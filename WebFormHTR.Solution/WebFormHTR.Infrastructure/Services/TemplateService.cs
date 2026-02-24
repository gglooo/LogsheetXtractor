using System.Text.Json;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Features.Residuals;
using WebFormHTR.Application.Features.ROIs;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Features.Template;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Features.Template.Interfaces;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Infrastructure.Services.Scripting.DTOs;

using Microsoft.Extensions.Logging;

namespace WebFormHTR.Infrastructure.Services;

public class TemplateService(
    IAppDbContext dbContext,
    IMapper mapper,
    IResidualService residualService,
    IRoiService roiService,
    IHtrScriptEngine scriptEngine,
    ILogger<TemplateService> logger
) : ITemplateService
{
    public async Task<TemplateDetailDto> CreateTemplateAsync(CreateTemplateCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating template {TemplateName}", command.Name);

        var backsideTemplate = command.Backside is not null
            ? command.Backside.ImportedConfig is not null
                ? GetTemplateFromImportedConfig(command.Backside.ImportedConfig, command.Backside.FileId,
                    command.Backside.Name, command.Backside.ParentId)
                : GetTemplate(command.Backside.Name, command.Backside.FileId, command.Backside.ParentId)
            : null;

        var template = command.ImportedConfig is not null
            ? GetTemplateFromImportedConfig(command.ImportedConfig, command.FileId, command.Name, command.ParentId)
            : GetTemplate(command.Name, command.FileId, command.ParentId);


        await dbContext.Templates.AddAsync(template, cancellationToken);

        template.SetBacksideTemplate(backsideTemplate);

        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Template {TemplateId} ('{TemplateName}') created successfully", template.Id, template.Name);

        var createdTemplate = await dbContext.Templates
            .AsNoTracking()
            .Include(t => t.File)
            .Include(t => t.Parent)
            .Include(t => t.BacksideTemplate)
            .FirstAsync(t => t.Id == template.Id, cancellationToken);

        return mapper.Map<TemplateDetailDto>(createdTemplate);
    }

    public async Task<TemplateDetailDto> CloneTemplateAsync(Guid templateId, string newTemplateName, Guid fileId,
        CloneTemplateBacksideCommand? backside, CancellationToken cancellationToken)
    {
        logger.LogInformation("Cloning template {TemplateId} to new name '{NewTemplateName}'", templateId, newTemplateName);
        var parentTemplate = await dbContext
            .Templates
            .AsNoTracking()
            .FirstAsync(t => t.Id == templateId, cancellationToken);
        var clonedTemplate = GetTemplate(newTemplateName, fileId, parentTemplate.Id);
        clonedTemplate.Id = Guid.NewGuid();

        await dbContext.Templates.AddAsync(clonedTemplate, cancellationToken);

        if (backside is not null)
        {
            var backsideTemplate = GetTemplate(backside.Name, backside.FileId, null);
            backsideTemplate.Id = Guid.NewGuid();

            await dbContext.Templates.AddAsync(backsideTemplate, cancellationToken);
            clonedTemplate.SetBacksideTemplate(backsideTemplate);

            if (parentTemplate.BacksideTemplateId.HasValue)
            {
                backsideTemplate.ParentId = parentTemplate.BacksideTemplateId.Value;
                await residualService.CloneResidualsForTemplateAsync(parentTemplate.BacksideTemplateId.Value, backsideTemplate.Id, cancellationToken);
                await roiService.CloneRoisForTemplateAsync(parentTemplate.BacksideTemplateId.Value, backsideTemplate.Id, cancellationToken);
            }
        }

        await residualService.CloneResidualsForTemplateAsync(parentTemplate.Id, clonedTemplate.Id, cancellationToken);
        await roiService.CloneRoisForTemplateAsync(parentTemplate.Id, clonedTemplate.Id, cancellationToken);
        
        logger.LogInformation("Template cloned successfully. New Template ID: {NewTemplateId}", clonedTemplate.Id);

        return mapper.Map<TemplateDetailDto>(clonedTemplate);
    }

    public async Task<string> ExportTemplateConfigAsync(Guid templateId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Exporting config for template {TemplateId}", templateId);
        var template = await dbContext.Templates
            .AsNoTracking()
            .Include(t => t.File)
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

        if (template is null)
        {
            logger.LogWarning("Template {TemplateId} not found for export", templateId);
            throw new Exception("Template not found");
        }

        var templateConfig = mapper.Map<PythonTemplateConfig>(template);
        var options = new JsonSerializerOptions { WriteIndented = true };
        return JsonSerializer.Serialize(templateConfig, options);
    }

    private PdfDimensionsDto CalculateTemplateFileDimensions(Guid fileId)
    {
        var file = dbContext.Files.FirstOrDefault(f => f.Id == fileId);
        if (file is null)
        {
            logger.LogError("File {FileId} not found", fileId);
            throw new Exception("File not found");
        }

        var dimensions = scriptEngine.GetPdfDimensionsAsync(file, CancellationToken.None).Result;
        if (dimensions is null)
        {
            logger.LogError("Unable to get PDF dimensions for file {FileId}", fileId);
            throw new Exception("Unable to get PDF dimensions");
        }

        return dimensions;
    }

    private Template GetTemplate(string templateName, Guid fileId, Guid? parentId)
    {
        var dimensions = CalculateTemplateFileDimensions(fileId);

        return new Template
        {
            Name = templateName,
            ParentId = parentId,
            FileId = fileId,
            Width = dimensions.Width,
            Height = dimensions.Height
        };
    }

    private Template GetTemplateFromImportedConfig(string importedConfig, Guid fileId, string templateName,
        Guid? parentId)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var deserialized = JsonSerializer.Deserialize<PythonTemplateConfig>(importedConfig, options);

            if (deserialized is null)
            {
                logger.LogError("Imported configuration deserialized to null");
                throw new Exception("Imported configuration is null");
            }

            var template = mapper.Map<Template>(deserialized);
            template.FileId = fileId;
            template.Name = templateName;
            template.ParentId = parentId;

            return template;
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Invalid imported configuration format");
            throw new Exception("Invalid imported configuration format", ex);
        }
    }
}