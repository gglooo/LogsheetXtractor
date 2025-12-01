using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Features.File.Interfaces;
using WebFormHTR.Application.Features.Residuals;
using WebFormHTR.Application.Features.ROIs;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Features.Template.Interfaces;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;

namespace WebFormHTR.Infrastructure.Services;

public class TemplateService(
    IAppDbContext dbContext,
    IMapper mapper,
    IResidualService residualService,
    IRoiService roiService) : ITemplateService
{
    public async Task<TemplateDetailDto> CloneTemplateAsync(Guid templateId, string newTemplateName, Guid fileId,
        CancellationToken cancellationToken)
    {
        var parentTemplate = await dbContext
            .Templates
            .AsNoTracking()
            .FirstAsync(t => t.Id == templateId, cancellationToken);
        var newId = Guid.NewGuid();

        var clonedTemplate = new Template
        {
            Name = newTemplateName,
            ParentId = parentTemplate.Id,
            FileId = fileId,
            Id = newId
        };

        await dbContext.Templates.AddAsync(clonedTemplate, cancellationToken);

        await residualService.CloneResidualsForTemplateAsync(parentTemplate.Id, newId, cancellationToken);
        await roiService.CloneRoisForTemplateAsync(parentTemplate.Id, newId, cancellationToken);

        return mapper.Map<TemplateDetailDto>(clonedTemplate);
    }
}