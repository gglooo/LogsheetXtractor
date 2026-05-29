using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.ExtractedValues.DTOs;
using LogsheetXtractor.Application.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using LogsheetXtractor.Domain.Enums;

namespace LogsheetXtractor.Application.Features.ExtractedValues;

public sealed record VerifyExtractedValueCommand(
    Guid Id,
    string? CorrectedValue
);

public static class VerifyExtractedValueHandler
{
    public static async Task<Result<ExtractedValueDto>> Handle(VerifyExtractedValueCommand request,
        IAppDbContext dbContext,
        IMapper mapper,
        CancellationToken ct)
    {
        var extractedValue = dbContext.ExtractedValues
            .Include(e => e.Roi)
            .Include(e => e.Logsheet)
            .FirstOrDefault(e => e.Id == request.Id);

        if (extractedValue is null)
        {
            return Result.Fail(new NotFoundError("Extracted value not found"));
        }

        if (extractedValue.Logsheet.Status == ELogSheetStatus.Completed)
        {
            return Result.Fail(new InvalidStateError("The logsheet is already completed and cannot be modified."));
        }

        extractedValue.Status = EVerificationStatus.Verified;
        if (!string.IsNullOrEmpty(request.CorrectedValue) && request.CorrectedValue != extractedValue.Value)
        {
            extractedValue.CorrectedValue = request.CorrectedValue;
        }

        await dbContext.SaveChangesAsync(ct);

        return Result.Ok(mapper.Map<ExtractedValueDto>(extractedValue));
    }
}