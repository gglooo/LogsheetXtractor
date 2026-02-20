using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Features.ExtractedValues.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.ExtractedValues;

public sealed record GetRandomUnverifiedExtractedValueQuery;

public static class GetRandomUnverifiedExtractedValueHandler
{
    public static async Task<Result<ExtractedValueDto?>> Handle(
        GetRandomUnverifiedExtractedValueQuery request,
        IAppDbContext dbContext,
        IMapper mapper,
        CancellationToken ct)
    {
        var extractedValue = await dbContext.ExtractedValues
            .AsNoTracking()
            .Include(e => e.Roi)
            .Include(e => e.Logsheet)
            .Where(e => e.Status == EVerificationStatus.Unverified &&
                        e.Logsheet.DeletedAt == null &&
                        e.Logsheet.Template.DeletedAt == null)
            .OrderBy(_ => EF.Functions.Random())
            .FirstOrDefaultAsync(ct);

        return Result.Ok(extractedValue is null ? null : mapper.Map<ExtractedValueDto>(extractedValue));
    }
}
