using FluentResults;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Features.RoiValidation.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.RoiValidation;

public sealed record GetPredefinedRoiValidationConditionsQuery(ERoiType? RoiType = null);

public static class GetPredefinedRoiValidationConditionsHandler
{
    public static async Task<Result<IReadOnlyList<PredefinedRoiValidationConditionDto>>> Handle(
        GetPredefinedRoiValidationConditionsQuery request,
        IAppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.PredefinedRoiValidationConditions
            .AsNoTracking()
            .AsQueryable();

        if (request.RoiType.HasValue)
        {
            query = query.Where(x => x.RoiType == request.RoiType.Value);
        }

        var items = await query
            .OrderBy(x => x.Label)
            .Select(x => new PredefinedRoiValidationConditionDto(
                x.Id,
                x.Code,
                x.Label,
                x.RoiType,
                x.Condition))
            .ToListAsync(cancellationToken);

        return Result.Ok<IReadOnlyList<PredefinedRoiValidationConditionDto>>(items);
    }
}
