using FluentResults;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Features.Template.Interfaces;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Template;

public sealed record CloneTemplateCommand(Guid TemplateId, string NewTemplateName);

public static class CloneTemplateHandler
{
    public static async Task<Result<TemplateDetailDto>> Handle(CloneTemplateCommand request,
        CancellationToken cancellationToken, ITemplateService templateService, IAppDbContext dbContext)
    {
        try
        {
            var existingTemplate = dbContext.Templates.FirstOrDefault(t => t.Id == request.TemplateId);
            if (existingTemplate is null)
            {
                return Result.Fail<TemplateDetailDto>(new NotFoundError("Cloned template not found"));
            }
            
            var clonedTemplate = await templateService.CloneTemplateAsync(request.TemplateId, request.NewTemplateName, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
            
            return Result.Ok(clonedTemplate);
        } catch (Exception ex)
        {
            return Result.Fail<TemplateDetailDto>(ex.Message);
        }
    }
}