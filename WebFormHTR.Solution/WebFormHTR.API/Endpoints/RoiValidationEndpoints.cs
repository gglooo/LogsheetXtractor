using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebFormHTR.API.Extensions;
using WebFormHTR.Application.Features.RoiValidation;
using WebFormHTR.Application.Features.RoiValidation.DTOs;
using WebFormHTR.Domain.Enums;
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

    [WolverineGet("/api/roi-validation/predefined-conditions")]
    [ProducesResponseType(200, Type = typeof(IReadOnlyList<PredefinedRoiValidationConditionDto>))]
    [ProducesResponseType(400)]
    public static async Task<IResult> GetPredefinedConditions(
        ERoiType? roiType,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken ct)
    {
        if (httpContext.Request.Query.ContainsKey("roiType") && !roiType.HasValue)
        {
            return Results.BadRequest(new[]
            {
                $"Invalid roiType '{httpContext.Request.Query["roiType"]}'."
            });
        }

        var query = new GetPredefinedRoiValidationConditionsQuery(roiType);
        var result = await bus.InvokeAsync<Result<IReadOnlyList<PredefinedRoiValidationConditionDto>>>(query, ct);
        return result.ToHttpResult();
    }
}
