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

namespace WebFormHTR.Infrastructure.Services;

public class TemplateService(
    IAppDbContext dbContext,
    IMapper mapper,
    IResidualService residualService,
    IRoiService roiService,
    IHtrScriptEngine scriptEngine
) : ITemplateService
{
    public async Task<TemplateDetailDto> CreateTemplateAsync(CreateTemplateCommand command,
        CancellationToken cancellationToken)
    {
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

        var createdTemplate = await dbContext.Templates
            .AsNoTracking()
            .Include(t => t.File)
            .Include(t => t.Parent)
            .Include(t => t.BacksideTemplate)
            .FirstAsync(t => t.Id == template.Id, cancellationToken);

        return mapper.Map<TemplateDetailDto>(createdTemplate);
    }

    public async Task<TemplateDetailDto> CloneTemplateAsync(Guid templateId, string newTemplateName, Guid fileId,
        CancellationToken cancellationToken)
    {
        var parentTemplate = await dbContext
            .Templates
            .AsNoTracking()
            .FirstAsync(t => t.Id == templateId, cancellationToken);
        var newId = Guid.NewGuid();

        var dimensions = CalculateTemplateFileDimensions(fileId);

        var clonedTemplate = new Template
        {
            Name = newTemplateName,
            ParentId = parentTemplate.Id,
            FileId = fileId,
            Id = newId,
            Width = dimensions.Width,
            Height = dimensions.Height
        };

        await dbContext.Templates.AddAsync(clonedTemplate, cancellationToken);

        await residualService.CloneResidualsForTemplateAsync(parentTemplate.Id, newId, cancellationToken);
        await roiService.CloneRoisForTemplateAsync(parentTemplate.Id, newId, cancellationToken);

        return mapper.Map<TemplateDetailDto>(clonedTemplate);
    }

    public async Task<string> ExportTemplateConfigAsync(Guid templateId, CancellationToken cancellationToken)
    {
        var template = await dbContext.Templates
            .AsNoTracking()
            .Include(t => t.File)
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

        if (template is null)
        {
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
            throw new Exception("File not found");
        }

        var dimensions = scriptEngine.GetPdfDimensionsAsync(file, CancellationToken.None).Result;
        if (dimensions is null)
        {
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
            throw new Exception("Invalid imported configuration format", ex);
        }
    }
}