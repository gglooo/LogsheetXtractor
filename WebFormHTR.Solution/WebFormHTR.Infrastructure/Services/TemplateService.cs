using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Features.File.Interfaces;
using WebFormHTR.Application.Features.Residuals;
using WebFormHTR.Application.Features.ROIs;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Features.Template;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Features.Template.Interfaces;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;

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
        var dimensions = CalculateTemplateFileDimensions(command.FileId);
        var template = new Template
        {
            Name = command.Name,
            ParentId = command.ParentId,
            FileId = command.FileId,
            Width = dimensions.Width,
            Height = dimensions.Height
        };

        await dbContext.Templates.AddAsync(template, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var createdTemplate = await dbContext.Templates
            .AsNoTracking()
            .Include(t => t.File)
            .Include(t => t.Parent)
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
}