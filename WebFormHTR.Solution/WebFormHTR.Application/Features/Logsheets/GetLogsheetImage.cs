using FluentResults;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.File.Interfaces;
using WebFormHTR.Application.Features.PdfCropper;
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

        var pdfStream = (await fileService.GetFileAsync(logsheet.FileId))?.Stream;
        if (pdfStream is null)
        {
            return Result.Fail(new NotFoundError("Logsheet file not found"));
        }

        var srcPoints = alignmentData;
        if (srcPoints?.Count != 4)
        {
            srcPoints = GetTemplateCorners(template, 1);
        }

        var outputScale = 2;
        var dstPoints = GetTemplateCorners(template, outputScale);

        var warpedStream = pdfCropperService.GetWarpedSection(pdfStream.ToByteArray(), 0, srcPoints, dstPoints,
            template.Width * outputScale ?? 0, template.Height * outputScale ?? 0,
            template.Width ?? 0, template.Height ?? 0, cancellationToken);

        return Result.Ok(new GetFileDto
        {
            FileName = $"logsheet_{logsheet.Id}.png",
            ContentType = "image/png",
            Stream = warpedStream
        });
    }

    private static List<PointCoordinate> GetTemplateCorners(Domain.Entities.Template template, int scale)
    {
        return
        [
            new PointCoordinate { X = 0, Y = 0 },
            new PointCoordinate { X = template.Width * scale ?? 0, Y = 0 },
            new PointCoordinate { X = template.Width * scale ?? 0, Y = template.Height * scale ?? 0 },
            new PointCoordinate { X = 0, Y = template.Height * scale ?? 0 }
        ];
    }
}