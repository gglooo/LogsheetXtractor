using FluentResults;
using LogsheetXtractor.API.Extensions;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.ExtractedValues;
using LogsheetXtractor.Application.Features.ExtractedValues.DTOs;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace LogsheetXtractor.API.Endpoints;

public static class ExtractedValueEndpoints
{
    [WolverineGet("/api/extracted-values/{id}/image")]
    [ProducesResponseType(200, Type = typeof(GetFileDto))]
    [ProducesResponseType(404)]
    public static async Task<IResult> GetExtractedValueImage(
        Guid id,
        IMessageBus bus,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        var query = new GetExtractedValueImageQuery(id);
        var result = await bus.InvokeAsync<Result<GetFileDto>>(query, ct);

        if (result.IsSuccess)
        {
            // The images are immutable, so we can set long-term caching headers
            httpContext.Response.Headers.Append(
                "Cache-Control",
                "public, max-age=31536000, immutable"
            );
        }

        return result.ToHttpResult();
    }

    [WolverinePost("/api/extracted-values/{id}/verify")]
    [ProducesResponseType(200, Type = typeof(ExtractedValueDto))]
    [ProducesResponseType(404)]
    public static async Task<IResult> VerifyExtractedValue(
        Guid id,
        VerifyExtractedValueDto request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var command = new VerifyExtractedValueCommand(id, request.CorrectedValue);
        var result = await bus.InvokeAsync<Result<ExtractedValueDto>>(command, ct);

        return result.ToHttpResult();
    }

    [WolverinePost("/api/extracted-values/batch/verify")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ExtractedValueDto>))]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public static async Task<IResult> BatchVerifyExtractedValues(
        BatchVerifyExtractedValuesCommand command,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<Result<IEnumerable<ExtractedValueDto>>>(command, ct);

        return result.ToHttpResult();
    }

    [WolverineGet("/api/extracted-values/unverified/random")]
    [ProducesResponseType(200, Type = typeof(ExtractedValueDto))]
    [ProducesResponseType(204)]
    public static async Task<IResult> GetRandomUnverifiedExtractedValue(
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var query = new GetRandomUnverifiedExtractedValueQuery();
        var result = await bus.InvokeAsync<Result<ExtractedValueDto?>>(query, ct);

        if (result.IsFailed)
        {
            return result.ToHttpResult();
        }

        return result.Value is null ? Results.NoContent() : Results.Ok(result.Value);
    }

    [WolverineGet("/api/extracted-values/unverified/next-logsheet")]
    [ProducesResponseType(200, Type = typeof(ExtractedValueDto))]
    [ProducesResponseType(204)]
    public static async Task<IResult> GetNextLogsheetUnverifiedExtractedValues(
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var query = new GetNextLogsheetUnverifiedExtractedValuesQuery();
        var result = await bus.InvokeAsync<Result<ExtractedValueDto?>>(query, ct);

        if (result.IsFailed)
        {
            return result.ToHttpResult();
        }

        return result.Value is null ? Results.NoContent() : Results.Ok(result.Value);
    }
}
