using FluentResults;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.File.Interfaces;
using WebFormHTR.Application.Features.PdfImage;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.ValueObjects;
using WebFormHTR.Infrastructure.Extensions;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record GetLogsheetImageQuery(Guid LogsheetId, bool IsFrontside);

public static class GetLogsheetImageHandler
{
    public static async Task<Result<GetFileDto>> Handle(
        GetLogsheetImageQuery query,
        IAppDbContext dbContext,
        ICoordinateTransformerService coordinateTransformerService,
        IPdfCropperService pdfCropperService,
        IFileService fileService,
        CancellationToken cancellationToken)
    {
        var logsheet = await dbContext.Logsheets.FirstOrDefaultAsync(l => l.Id == query.LogsheetId, cancellationToken);
        if (logsheet is null)
        {
            return Result.Fail<GetFileDto>(new NotFoundError("Logsheet not found"));
        }

        var alignmentData = query.IsFrontside
            ? logsheet.AlignmentDataModelConfig.Frontside
            : logsheet.AlignmentDataModelConfig.Backside;
        var template = query.IsFrontside
            ? logsheet.Template
            : logsheet.BacksideTemplate;

        if (template is null)
        {
            return Result.Fail<GetFileDto>("Logsheet template not found");
        }

        // This is a little hack -- we transform the entire logsheet area to get its aligned coordinates
        var alignedLogsheetCoordinates = coordinateTransformerService.TransformCoordinates(new Coordinates
        {
            X = 0,
            Y = 0,
            Width = template.Width ?? 0,
            Height = template.Height ?? 0
        }, new Coordinates
        {
            X = 0,
            Y = 0,
            Width = template.Width ?? 0,
            Height = template.Height ?? 0
        }, alignmentData);

        var pdfStream = (await fileService.GetFileAsync(logsheet.FileId))?.Stream;
        if (pdfStream is null)
        {
            return Result.Fail(new NotFoundError("Logsheet file not found"));
        }

        var logsheetPdfStream = pdfCropperService.GetCroppedSection(pdfStream.ToByteArray(), 0,
            alignedLogsheetCoordinates.X, alignedLogsheetCoordinates.Y,
            alignedLogsheetCoordinates.Width,
            alignedLogsheetCoordinates.Height, (int)template.Width!,
            (int)template.Height!, cancellationToken);

        return Result.Ok(new GetFileDto
        {
            FileName = $"logsheet_{logsheet.Id}.png",
            ContentType = "image/png",
            Stream = logsheetPdfStream
        });
    }
}