using FluentResults;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.ROIs;

public sealed record SetTemplateRoisCommand(
    Guid TemplateId,
    IEnumerable<SetRoiDto> Rois
);

public static class SetTemplateRoisHandler
{
    public static async Task<Result<IEnumerable<RoiDto>>> Handle(SetTemplateRoisCommand request, IRoiService roiService,
        IAppDbContext dbContext, CancellationToken ct)
    {
        var templateEditStatus = await dbContext.Templates
            .Where(t => t.Id == request.TemplateId)
            .Select(TemplateRules.IsEditable)
            .ToListAsync(ct);

        if (!templateEditStatus.Any())
        {
            return Result.Fail<IEnumerable<RoiDto>>(new NotFoundError("Template not found"));
        }

        if (!templateEditStatus[0])
        {
            return Result.Fail(new InvalidStateError("Template is not editable"));
        }

        try
        {
            var updatedRois = await roiService.SetRoisForTemplateAsync(request.TemplateId, request.Rois, ct);
            await dbContext.SaveChangesAsync(ct);

            return Result.Ok(updatedRois);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to update ROIs: {ex.Message}");
        }
    }
}