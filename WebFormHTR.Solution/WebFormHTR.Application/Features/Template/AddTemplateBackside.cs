using FluentResults;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Features.Template.Interfaces;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Application.Rules;

namespace WebFormHTR.Application.Features.Template;

public sealed record AddTemplateBacksideCommand(Guid TemplateId, string Name, Guid FileId);

public static class AddTemplateBacksideHandler
{
    public static async Task<Result<TemplateDetailDto>> Handle(
        AddTemplateBacksideCommand request,
        ITemplateService templateService,
        IAppDbContext dbContext,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Fail<TemplateDetailDto>(new ValidationError("Backside template name is required"));
        }

        var templateState = await dbContext.Templates
            .Where(t => t.Id == request.TemplateId)
            .Select(t => new
            {
                t.Id,
                t.BacksideTemplateId,
                HasFrontsideTemplate = t.FrontsideTemplate != null
            })
            .FirstOrDefaultAsync(ct);

        if (templateState is null)
        {
            return Result.Fail<TemplateDetailDto>(new NotFoundError("Template not found"));
        }

        if (templateState.HasFrontsideTemplate)
        {
            return Result.Fail<TemplateDetailDto>(
                new InvalidStateError("Cannot add backside to a backside template"));
        }

        if (templateState.BacksideTemplateId.HasValue)
        {
            return Result.Fail<TemplateDetailDto>(new InvalidStateError("Template already has a backside"));
        }

        var templateEditStatus = await dbContext.Templates
            .Where(t => t.Id == request.TemplateId)
            .Select(TemplateRules.IsEditable)
            .ToListAsync(ct);

        if (!templateEditStatus[0])
        {
            return Result.Fail<TemplateDetailDto>(new InvalidStateError("Template is not editable"));
        }

        if (!await dbContext.Files.AnyAsync(f => f.Id == request.FileId, ct))
        {
            return Result.Fail<TemplateDetailDto>(new NotFoundError("Backside file not found"));
        }

        var normalizedName = request.Name.Trim();
        if (await dbContext.Templates.AnyAsync(t => t.Name == normalizedName, ct))
        {
            return Result.Fail<TemplateDetailDto>(
                new ValidationError("A template with this name already exists."));
        }

        try
        {
            var result = await templateService.AddBacksideTemplateAsync(
                request.TemplateId,
                normalizedName,
                request.FileId,
                ct);
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
