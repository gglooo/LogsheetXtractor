using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Template.DTOs;
using LogsheetXtractor.Application.Features.Template.Interfaces;
using LogsheetXtractor.Application.Interfaces;

namespace LogsheetXtractor.Application.Features.Template;

public sealed record CloneTemplateBacksideCommand(string Name, Guid FileId);

public sealed record CloneTemplateCommand(
    Guid TemplateId,
    string NewTemplateName,
    Guid FileId,
    CloneTemplateBacksideCommand? Backside = null
);

public static class CloneTemplateHandler
{
    public static async Task<Result<TemplateDetailDto>> Handle(
        CloneTemplateCommand request,
        CancellationToken cancellationToken,
        ITemplateService templateService,
        IAppDbContext dbContext
    )
    {
        try
        {
            var existingTemplate = dbContext.Templates.FirstOrDefault(t =>
                t.Id == request.TemplateId
            );
            if (existingTemplate is null)
            {
                return Result.Fail<TemplateDetailDto>(
                    new NotFoundError("Cloned template not found")
                );
            }

            var existingFile = dbContext.Files.FirstOrDefault(t => t.Id == existingTemplate.FileId);
            if (existingFile is null)
            {
                return Result.Fail<TemplateDetailDto>(
                    new NotFoundError("Cloned template's file not found")
                );
            }

            var clonedTemplateResult = await templateService.CloneTemplateAsync(
                request.TemplateId,
                request.NewTemplateName,
                request.FileId,
                request.Backside,
                cancellationToken
            );

            if (clonedTemplateResult.IsFailed)
            {
                return clonedTemplateResult.ToResult();
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(clonedTemplateResult.Value);
        }
        catch (Exception ex)
        {
            return Result.Fail<TemplateDetailDto>(ex.Message);
        }
    }
}
