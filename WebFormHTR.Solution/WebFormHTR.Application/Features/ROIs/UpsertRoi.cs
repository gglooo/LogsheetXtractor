using FluentResults;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.ROIs;

public sealed record UpsertRoiCommand(
    Guid TemplateId,
    UpsertRoiDto Roi
);

public static class UpsertRoiHandler
{
    public static async Task<Result<RoiDto>> Handle(UpsertRoiCommand request, IAppDbContext dbContext,
        IRoiService roiService, CancellationToken ct)
    {
        try
        {
            var template = await dbContext.Templates.FindAsync(request.TemplateId, ct);
            if (template is null)
            {
                return Result.Fail<RoiDto>(new NotFoundError("Template not found"));
            }

            var upsertedRoiResult = await roiService.UpsertRoiForTemplateAsync(request.TemplateId, request.Roi, ct);
            if (upsertedRoiResult.IsFailed)
            {
                return upsertedRoiResult.ToResult();
            }

            await dbContext.SaveChangesAsync(ct);

            return upsertedRoiResult;
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to upsert ROI: {ex.Message}");
        }
    }
}