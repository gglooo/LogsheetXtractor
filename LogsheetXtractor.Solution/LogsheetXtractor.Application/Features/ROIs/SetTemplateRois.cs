using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.ROIs.DTOs;
using LogsheetXtractor.Application.Features.RoiValidation;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Application.Rules;
using Microsoft.EntityFrameworkCore;

namespace LogsheetXtractor.Application.Features.ROIs;

public sealed record SetTemplateRoisCommand(Guid TemplateId, IEnumerable<SetRoiDto> Rois);

public static class SetTemplateRoisHandler
{
    public static async Task<Result<IEnumerable<RoiDto>>> Handle(
        SetTemplateRoisCommand request,
        IRoiService roiService,
        IAppDbContext dbContext,
        IRoiValidationConditionTreeValidator conditionTreeValidator,
        CancellationToken ct
    )
    {
        var templateEditStatus = await dbContext
            .Templates.Where(t => t.Id == request.TemplateId)
            .Select(TemplateRules.IsEditable)
            .ToListAsync(ct);

        if (templateEditStatus.Count == 0)
        {
            return Result.Fail<IEnumerable<RoiDto>>(new NotFoundError("Template not found"));
        }

        if (!templateEditStatus[0])
        {
            return Result.Fail(new InvalidStateError("Template is not editable"));
        }

        var requestRois = request.Rois.ToList();
        for (var i = 0; i < requestRois.Count; i++)
        {
            var roi = requestRois[i];
            if (roi.ValidationCondition is null)
            {
                continue;
            }

            if (roi.Type is null)
            {
                return Result.Fail<IEnumerable<RoiDto>>(
                    new ValidationError(
                        $"ROI at index {i} ('{roi.VariableName}') must define a type when validationCondition is set."
                    )
                );
            }

            var validationResult = conditionTreeValidator.Validate(
                roi.Type.Value,
                roi.ValidationCondition
            );
            if (validationResult.IsFailed)
            {
                var message =
                    validationResult.Errors.FirstOrDefault()?.Message
                    ?? "Invalid validation condition.";
                return Result.Fail<IEnumerable<RoiDto>>(
                    new ValidationError(
                        $"ROI at index {i} ('{roi.VariableName}') has invalid validationCondition: {message}"
                    )
                );
            }
        }

        try
        {
            var updatedRoisResult = await roiService.SetRoisForTemplateAsync(
                request.TemplateId,
                requestRois,
                ct
            );
            if (updatedRoisResult.IsFailed)
            {
                return updatedRoisResult.ToResult();
            }

            await dbContext.SaveChangesAsync(ct);

            return Result.Ok(updatedRoisResult.Value);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to update ROIs: {ex.Message}");
        }
    }
}
