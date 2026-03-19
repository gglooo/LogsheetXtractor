using FluentResults;
using WebFormHTR.Application.Features.RoiValidation.DTOs;

namespace WebFormHTR.Application.Features.RoiValidation;

public sealed record GetRoiValidationRuleCatalogQuery;

public static class GetRoiValidationRuleCatalogHandler
{
    public static Task<Result<RoiValidationRuleCatalogDto>> Handle(
        GetRoiValidationRuleCatalogQuery request,
        IRoiValidationRuleCatalogProvider catalogProvider,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Ok(catalogProvider.GetCatalog()));
    }
}
