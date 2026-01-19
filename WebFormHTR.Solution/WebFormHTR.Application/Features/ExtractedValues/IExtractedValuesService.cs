using FluentResults;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Domain.Entities;

namespace WebFormHTR.Application.Features.ExtractedValues;

public interface IExtractedValuesService
{
    Task<Result<GetFileDto>> GetExtractedValueImageAsync(ExtractedValue extractedValue, CancellationToken ct);
}