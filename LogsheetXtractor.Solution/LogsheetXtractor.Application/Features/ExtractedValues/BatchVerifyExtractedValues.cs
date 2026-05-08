using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.ExtractedValues.DTOs;
using LogsheetXtractor.Application.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using LogsheetXtractor.Domain.Enums;

namespace LogsheetXtractor.Application.Features.ExtractedValues;

public sealed record BatchVerifyExtractedValuesCommand(Guid[] Ids);

public static class BatchVerifyExtractedValuesHandler
{
    public static async Task<Result<IEnumerable<ExtractedValueDto>>> Handle(BatchVerifyExtractedValuesCommand request,
        IAppDbContext dbContext,
        IMapper mapper,
        CancellationToken ct)
    {
        var extractedValues = await dbContext.ExtractedValues
            .Include(e => e.Roi)
            .Include(e => e.Logsheet)
            .Where(e => request.Ids.Contains(e.Id))
            .ToListAsync(ct);

        if (extractedValues.Count != request.Ids.Length)
        {
            return Result.Fail(new NotFoundError("One or more extracted values not found"));
        }

        foreach (var extractedValue in extractedValues)
        {
            if (extractedValue.Logsheet.Status == ELogSheetStatus.Completed)
            {
                return Result.Fail(new InvalidStateError($"Logsheet for extracted value {extractedValue.Id} is already completed."));
            }

            extractedValue.Status = EVerificationStatus.Verified;
        }

        await dbContext.SaveChangesAsync(ct);

        return Result.Ok(mapper.Map<IEnumerable<ExtractedValueDto>>(extractedValues));
    }
}
