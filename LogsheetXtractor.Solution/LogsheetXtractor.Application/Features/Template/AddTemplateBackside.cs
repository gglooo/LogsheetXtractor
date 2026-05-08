using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.Template.DTOs;
using LogsheetXtractor.Application.Features.Template.Interfaces;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Application.Rules;
using Microsoft.EntityFrameworkCore;

namespace LogsheetXtractor.Application.Features.Template;

public sealed record AddTemplateBacksideCommand(Guid TemplateId, Guid FileId);

public static class AddTemplateBacksideHandler
{
    public static async Task<Result<TemplateDetailDto>> Handle(
        AddTemplateBacksideCommand request,
        ITemplateService templateService,
        IAppDbContext dbContext,
        CancellationToken ct
    )
    {
        var templateState = await dbContext
            .Templates.Where(t => t.Id == request.TemplateId)
            .Select(t => new
            {
                t.Id,
                t.BacksideTemplateId,
                HasFrontsideTemplate = t.FrontsideTemplate != null,
            })
            .FirstOrDefaultAsync(ct);

        if (templateState is null)
        {
            return Result.Fail<TemplateDetailDto>(new NotFoundError("Template not found"));
        }

        if (templateState.HasFrontsideTemplate)
        {
            return Result.Fail<TemplateDetailDto>(
                new InvalidStateError("Cannot add backside to a backside template")
            );
        }

        if (templateState.BacksideTemplateId.HasValue)
        {
            return Result.Fail<TemplateDetailDto>(
                new InvalidStateError("Template already has a backside")
            );
        }

        var templateEditStatus = await dbContext
            .Templates.Where(t => t.Id == request.TemplateId)
            .Select(TemplateRules.IsEditable)
            .ToListAsync(ct);

        if (!templateEditStatus[0])
        {
            return Result.Fail<TemplateDetailDto>(
                new InvalidStateError("Template is not editable")
            );
        }

        if (!await dbContext.Files.AnyAsync(f => f.Id == request.FileId, ct))
        {
            return Result.Fail<TemplateDetailDto>(new NotFoundError("Backside file not found"));
        }

        try
        {
            var result = await templateService.AddBacksideTemplateAsync(
                request.TemplateId,
                request.FileId,
                ct
            );
            if (result.IsFailed)
            {
                return result.ToResult();
            }

            await dbContext.SaveChangesAsync(ct);
            return Result.Ok(result.Value);
        }
        catch (Exception e)
        {
            return Result.Fail<TemplateDetailDto>(e.Message);
        }
    }
}
