using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Features.ExtractedValues.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.ExtractedValues;

public sealed record GetNextLogsheetUnverifiedExtractedValuesQuery;

public static class GetNextLogsheetUnverifiedExtractedValuesHandler
{
    public static async Task<Result<ExtractedValueDto?>> Handle(
        GetNextLogsheetUnverifiedExtractedValuesQuery request,
        IAppDbContext dbContext,
        IMapper mapper,
        CancellationToken ct)
    {
        var next = await dbContext.ExtractedValues
            .AsNoTracking()
            .Include(e => e.Roi)
            .Include(e => e.Logsheet)
            .Where(e => e.Status == EVerificationStatus.Unverified &&
                        e.Logsheet.DeletedAt == null &&
                        e.Logsheet.Template.DeletedAt == null)
            .OrderBy(e => e.LogsheetId)
            .ThenBy(e => e.CreatedAt)
            .FirstOrDefaultAsync(ct);

        return Result.Ok(next is null ? null : mapper.Map<ExtractedValueDto>(next));
    }
}
