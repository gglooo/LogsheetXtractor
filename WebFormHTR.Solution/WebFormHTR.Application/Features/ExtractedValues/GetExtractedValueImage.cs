using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.File.Interfaces;
using WebFormHTR.Application.Features.PdfImage;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Infrastructure.Extensions;

namespace WebFormHTR.Application.Features.ExtractedValues;

public sealed record GetExtractedValueImageQuery(
    Guid Id
);

public static class GetExtractedValueImageHandler
{
    public static async Task<Result<GetFileDto>> Handle(GetExtractedValueImageQuery request,
        IAppDbContext dbContext,
        IMapper mapper,
        IPdfCropperService pdfCropperService,
        IFileService fileService,
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

        var pdfStream = (await fileService.GetFileAsync(extractedValue.Logsheet.FileId))?.Stream;
        if (pdfStream is null)
        {
            return Result.Fail(new NotFoundError("Logsheet file not found"));
        }

        var logsheetPdfStream = pdfCropperService.GetCroppedSection(pdfStream.ToByteArray(), 0,
            (int)extractedValue.Roi.Coordinates.X,
            (int)extractedValue.Roi.Coordinates.Y, (int)extractedValue.Roi.Coordinates.Width,
            (int)extractedValue.Roi.Coordinates.Height, (int)extractedValue.Logsheet.Template.Width,
            (int)extractedValue.Logsheet.Template.Height, cancellationToken);

        return Result.Ok(new GetFileDto
        {
            FileName = $"extracted_value_{extractedValue.Id}.png",
            ContentType = "image/png",
            Stream = logsheetPdfStream
        });
    }
}