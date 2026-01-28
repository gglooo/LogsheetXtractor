using FluentResults;
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
            .Include(ev => ev.Logsheet)
            .ThenInclude(l => l.Template)
            .Include(ev => ev.Roi)
            .FirstOrDefaultAsync(ev => ev.Id == request.Id, cancellationToken);

        if (extractedValue is null)
        {
            return Result.Fail(new NotFoundError("Extracted value not found"));
        }

        return await extractedValuesService.GetExtractedValueImageAsync(extractedValue, cancellationToken);
    }
}