using FluentResults;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Extensions;
using WebFormHTR.Application.Features.File.Interfaces;
using WebFormHTR.Application.Features.PdfCropper;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Application.Features.Logsheets;

public sealed record GetLogsheetImageQuery(Guid LogsheetId, bool IsFrontside);

public static class GetLogsheetImageHandler
{
    public static async Task<Result<GetFileDto>> Handle(
        GetLogsheetImageQuery query,
        IAppDbContext dbContext,
        IPdfCropperService pdfCropperService,
        IFileService fileService,
        CancellationToken cancellationToken)
    {
        var logsheet = await dbContext.Logsheets
            .AsNoTracking()
            .Include(l => l.Template)
            .FirstOrDefaultAsync(l => l.Id == query.LogsheetId, cancellationToken);
        if (logsheet is null)
        {
            return Result.Fail<GetFileDto>(new NotFoundError("Logsheet not found"));
        }

        var backsideTemplate = !query.IsFrontside
            ? await dbContext.Templates
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == logsheet.Template.BacksideTemplateId, cancellationToken)
            : null;

        var alignmentData = query.IsFrontside
            ? logsheet.AlignmentData.Frontside
            : logsheet.AlignmentData.Backside;
        var template = query.IsFrontside
            ? logsheet.Template
            : backsideTemplate;

        if (template is null)
        {
            return Result.Fail<GetFileDto>("Logsheet template not found");
        }

        var pdfStream = (await fileService.GetFileAsync(logsheet.FileId))?.Stream;
        if (pdfStream is null)
        {
            return Result.Fail(new NotFoundError("Logsheet file not found"));
        }

        var pdfBytes = pdfStream.ToByteArray();

        if (!query.IsFrontside)
        {
            var pageCount = pdfCropperService.GetPageCount(pdfBytes, cancellationToken);
            if (pageCount < 2)
            {
                return Result.Fail(new NotFoundError("Logsheet does not have a backside page"));
            }
        }

        var srcPoints = alignmentData;
        if (srcPoints?.Count != 4)
        {
            srcPoints = GetTemplateCorners(template, 1);
        }

        var outputScale = 2;
        var dstPoints = GetTemplateCorners(template, outputScale);
        var page = query.IsFrontside ? 0 : 1;

        var warpedStreamResult = pdfCropperService.GetWarpedSection(pdfBytes, page, srcPoints,
            dstPoints,
            template.Width * outputScale ?? 0, template.Height * outputScale ?? 0,
            template.Width ?? 0, template.Height ?? 0, cancellationToken);

        if (warpedStreamResult.IsFailed)
        {
            return warpedStreamResult.ToResult();
        }

        return Result.Ok(new GetFileDto
        {
            FileName = $"logsheet_{logsheet.Id}.png",
            ContentType = "image/png",
            Stream = warpedStreamResult.Value
        });
    }

    private static List<PointCoordinate> GetTemplateCorners(Domain.Entities.Template template, int scale)
    {
        return
        [
            new PointCoordinate(0, 0),
            new PointCoordinate(template.Width * scale ?? 0, 0),
            new PointCoordinate(template.Width * scale ?? 0, template.Height * scale ?? 0),
            new PointCoordinate(0, template.Height * scale ?? 0)
        ];
    }
}