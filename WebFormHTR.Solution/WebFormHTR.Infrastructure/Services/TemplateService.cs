using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Features.File.Interfaces;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Features.Template.Interfaces;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;

namespace WebFormHTR.Infrastructure.Services;

public class TemplateService(IAppDbContext dbContext, IMapper mapper, IFileService fileService): ITemplateService
{
    public async Task<TemplateDetailDto> CloneTemplateAsync(Guid templateId, string newTemplateName, CancellationToken cancellationToken)
    {
        var parentTemplate = await dbContext
            .Templates
            .AsNoTracking()
            .FirstAsync(t => t.Id == templateId, cancellationToken: cancellationToken);

        var clonedTemplate = new Template
        {
            Name = newTemplateName,
            ParentId = parentTemplate.Id,
            // TODO: figure out which other properties will need to get deep cloned
            // definitely ROI's and Residuals? The user will then have to manually
            // review them and adjust them if needed.
            Id = Guid.NewGuid()
        };

        if (parentTemplate.FileId is not null)
        {
            var clonedFile = await fileService.CloneFileAsync(parentTemplate.FileId.Value);
            clonedTemplate.FileId = clonedFile.Id;
        }
        
        await dbContext.Templates.AddAsync(clonedTemplate, cancellationToken);
        
        return mapper.Map<TemplateDetailDto>(clonedTemplate);
    }
}