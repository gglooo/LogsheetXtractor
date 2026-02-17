using FluentResults;
using Microsoft.AspNetCore.Mvc;
using WebFormHTR.API.Extensions;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.ExtractedValues;
using WebFormHTR.Application.Features.ExtractedValues.DTOs;
using Wolverine;
using Wolverine.Http;

namespace WebFormHTR.API.Endpoints;

public static class ExtractedValueEndpoints
{
    [WolverineGet("/api/extracted-values/{id}/image")]
    [ProducesResponseType(200, Type = typeof(GetFileDto))]
    [ProducesResponseType(404)]
    public static async Task<IResult> GetExtractedValueImage(Guid id, IMessageBus bus, HttpContext httpContext,
        CancellationToken ct)
    {
        // The images are immutable, so we can set long-term caching headers
        httpContext.Response.Headers.Append("Cache-Control", "public, max-age=31536000, immutable");

        var query = new GetExtractedValueImageQuery(id);
        var result = await bus.InvokeAsync<Result<GetFileDto>>(query, ct);

        return result.ToHttpResult();
    }

    [WolverinePost("/api/extracted-values/{id}/verify")]
    [ProducesResponseType(200, Type = typeof(ExtractedValueDto))]
    [ProducesResponseType(404)]
    public static async Task<IResult> VerifyExtractedValue(Guid id, VerifyExtractedValueDto request, IMessageBus bus,
        CancellationToken ct)
    {
        var command = new VerifyExtractedValueCommand(id, request.CorrectedValue);
        var result = await bus.InvokeAsync<Result<ExtractedValueDto>>(command, ct);

        return result.ToHttpResult();
    }
    [WolverinePost("/api/extracted-values/batch/verify")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ExtractedValueDto>))]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public static async Task<IResult> BatchVerifyExtractedValues(BatchVerifyExtractedValuesCommand command, IMessageBus bus,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<IEnumerable<ExtractedValueDto>>>(command, ct);

        return result.ToHttpResult();
    }
}