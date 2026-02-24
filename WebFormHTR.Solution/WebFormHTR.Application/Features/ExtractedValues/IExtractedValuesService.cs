using FluentResults;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Application.Features.ExtractedValues;

public sealed record GetExtractedValueImageDto(
    Guid ExtractedValueId,
    Guid LogsheetFileId,
    Coordinates RoiCoordinates,
    int TemplateWidth,
    int TemplateHeight,
    List<PointCoordinate>? AlignmentData,
    int PageNumber = 0
);

public interface IExtractedValuesService
{
    Task<Result<GetFileDto>> GetExtractedValueImageAsync(GetExtractedValueImageDto extractedValueDto,
        CancellationToken ct);
}