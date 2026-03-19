using FluentResults;
using Microsoft.AspNetCore.Mvc;
using WebFormHTR.API.Extensions;
using WebFormHTR.Application.Features.RoiValidation;
using WebFormHTR.Application.Features.RoiValidation.DTOs;
using Wolverine;
using Wolverine.Http;

namespace WebFormHTR.API.Endpoints;

public static class RoiValidationEndpoints
{
    [WolverineGet("/api/roi-validation/rules")]
    [ProducesResponseType(200, Type = typeof(RoiValidationRuleCatalogDto))]
    public static async Task<IResult> GetRuleCatalog(
        IMessageBus bus,
        CancellationToken ct)
    {
        var query = new GetRoiValidationRuleCatalogQuery();
        var result = await bus.InvokeAsync<Result<RoiValidationRuleCatalogDto>>(query, ct);
        return result.ToHttpResult();
    }
}
