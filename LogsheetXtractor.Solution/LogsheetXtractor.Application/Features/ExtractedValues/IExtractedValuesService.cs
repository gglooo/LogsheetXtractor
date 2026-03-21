using FluentResults;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Application.Features.ExtractedValues;

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