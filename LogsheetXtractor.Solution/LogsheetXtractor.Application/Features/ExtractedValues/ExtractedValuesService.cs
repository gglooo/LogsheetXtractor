using FluentResults;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Extensions;
using LogsheetXtractor.Application.Features.File.Interfaces;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Features.PdfCropper;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LogsheetXtractor.Application.Features.ExtractedValues;

public class ExtractedValuesService(
    IFileService fileService,
    IPdfCropperService pdfCropperService,
    ICoordinateTransformerService coordinateTransformer,
    ILogger<ExtractedValuesService> logger
) : IExtractedValuesService
{
    public async Task<Result<GetFileDto>> GetExtractedValueImageAsync(
        GetExtractedValueImageDto extractedValueDto,
        CancellationToken ct
    )
    {
        logger.LogInformation(
            "Getting extracted value image. ExtractedValueId: {Id}",
            extractedValueDto.ExtractedValueId
        );

        var pdfStream = (await fileService.GetFileAsync(extractedValueDto.LogsheetFileId))?.Stream;
        if (pdfStream is null)
        {
            logger.LogWarning(
                "Logsheet file not found for ExtractedValueId: {Id}",
                extractedValueDto.ExtractedValueId
            );
            return Result.Fail(new NotFoundError("Logsheet file not found"));
        }

        var alignedRoiCoordinates = coordinateTransformer.TransformCoordinates(
            extractedValueDto.RoiCoordinates,
            new LogsheetXtractor.Domain.ValueObjects.Coordinates(
                0,
                0,
                extractedValueDto.TemplateWidth,
                extractedValueDto.TemplateHeight
            ),
            extractedValueDto.AlignmentData
        );

        var logsheetPdfStreamResult = pdfCropperService.GetCroppedSection(
            pdfStream.ToByteArray(),
            extractedValueDto.PageNumber,
            alignedRoiCoordinates.X,
            alignedRoiCoordinates.Y,
            alignedRoiCoordinates.Width,
            alignedRoiCoordinates.Height,
            extractedValueDto.TemplateWidth,
            extractedValueDto.TemplateHeight,
            ct
        );

        if (logsheetPdfStreamResult.IsFailed)
        {
            return logsheetPdfStreamResult.ToResult();
        }

        return Result.Ok(
            new GetFileDto
            {
                FileName = $"extracted_value_{extractedValueDto.ExtractedValueId}.png",
                ContentType = "image/png",
                Stream = logsheetPdfStreamResult.Value,
            }
        );
    }
}
