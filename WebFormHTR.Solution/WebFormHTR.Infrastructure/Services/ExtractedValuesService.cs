using FluentResults;
using Microsoft.Extensions.Logging;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Extensions;
using WebFormHTR.Application.Features.ExtractedValues;
using WebFormHTR.Application.Features.File.Interfaces;
using WebFormHTR.Application.Features.PdfCropper;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;

namespace WebFormHTR.Infrastructure.Services;

public class ExtractedValuesService(
    IFileService fileService,
    IPdfCropperService pdfCropperService,
    ICoordinateTransformerService coordinateTransformer,
    ILogger<ExtractedValuesService> logger)
    : IExtractedValuesService
{
    public async Task<Result<GetFileDto>> GetExtractedValueImageAsync(ExtractedValue extractedValue,
        CancellationToken ct)
    {
        logger.LogInformation("Getting extracted value image. ExtractedValueId: {Id}", extractedValue.Id);
        var pdfStream = (await fileService.GetFileAsync(extractedValue.Logsheet.FileId))?.Stream;
        if (pdfStream is null)
        {
            logger.LogWarning("Logsheet file not found for ExtractedValueId: {Id}", extractedValue.Id);
            return Result.Fail(new NotFoundError("Logsheet file not found"));
        }

        // TODO: support backside as well
        var alignedRoiCoordinates =
            coordinateTransformer.TransformCoordinates(extractedValue.Roi.Coordinates,
                new Domain.ValueObjects.Coordinates
                {
                    X = 0,
                    Y = 0,
                    Width = extractedValue.Logsheet.Template.Width ?? 0,
                    Height = extractedValue.Logsheet.Template.Height ?? 0
                }, extractedValue.Logsheet.AlignmentDataModelConfig.Frontside);

        var logsheetPdfStream = pdfCropperService.GetCroppedSection(pdfStream.ToByteArray(), 0,
            alignedRoiCoordinates.X, alignedRoiCoordinates.Y,
            alignedRoiCoordinates.Width,
            alignedRoiCoordinates.Height, (int)extractedValue.Logsheet.Template.Width!,
            (int)extractedValue.Logsheet.Template.Height!, ct);

        return Result.Ok(new GetFileDto
        {
            FileName = $"extracted_value_{extractedValue.Id}.png",
            ContentType = "image/png",
            Stream = logsheetPdfStream
        });
    }
}