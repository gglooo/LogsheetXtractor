using FluentResults;
using ImTools;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.File.Interfaces;
using WebFormHTR.Application.Features.PdfCropper;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Application.Features.ExtractedValues;

public sealed record GetExtractedValueImageQuery(
    Guid Id
);

public static class GetExtractedValueImageHandler
{
    public static async Task<Result<GetFileDto>> Handle(GetExtractedValueImageQuery request,
        IAppDbContext dbContext,
        IExtractedValuesService extractedValuesService,
        CancellationToken cancellationToken)
    {
        var extractedValue = await dbContext.ExtractedValues
            .AsNoTracking()
            .Where(ev => ev.Id == request.Id)
            .Select(ev => new
            {
                ExtractedValueId = ev.Id,
                LogsheetFileId = ev.Logsheet.FileId,
                RoiCoordinates = ev.Roi.Coordinates,

                TemplateWidth = ev.Roi.Template.FrontsideTemplate != null
                    ? ev.Logsheet.Template.BacksideTemplate!.Width
                    : ev.Logsheet.Template.Width,

                TemplateHeight = ev.Roi.Template.FrontsideTemplate != null
                    ? ev.Logsheet.Template.BacksideTemplate!.Height
                    : ev.Logsheet.Template.Height,

                AlignmentData = ev.Roi.Template.FrontsideTemplate != null
                    ? ev.Logsheet.AlignmentData.Backside
                    : ev.Logsheet.AlignmentData.Frontside,

                PageNumber = ev.Roi.Template.FrontsideTemplate != null ? 1 : 0
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (extractedValue is null)
        {
            return Result.Fail(new NotFoundError("Extracted value not found"));
        }

        return await extractedValuesService.GetExtractedValueImageAsync(
            extractedValue.Adapt<GetExtractedValueImageDto>(), cancellationToken);
    }
}