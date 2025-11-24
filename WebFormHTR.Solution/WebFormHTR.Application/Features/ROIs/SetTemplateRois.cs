using FluentResults;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.ROIs;

public sealed record SetTemplateRoisCommand
(
    Guid TemplateId,
    IEnumerable<SetRoiDto> Rois
);


public static class SetTemplateRoisHandler
{
    public static async Task<Result<IEnumerable<RoiDto>>> Handle(SetTemplateRoisCommand request, IRoiService roiService, IAppDbContext dbContext, CancellationToken ct)
    {
        var template = await dbContext.Templates.FindAsync(request.TemplateId, ct);
        if (template is null)
        {
            return Result.Fail(new NotFoundError("Template not found"));
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