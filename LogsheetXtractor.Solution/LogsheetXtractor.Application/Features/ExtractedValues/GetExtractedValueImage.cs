using FluentResults;
using ImTools;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Interfaces;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using LogsheetXtractor.Application.Features.File.Interfaces;
using LogsheetXtractor.Application.Features.PdfCropper;
using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Application.Features.ExtractedValues;

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