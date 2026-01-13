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
    public static async Task<IResult> GetExtractedValueImage(Guid id, IMessageBus bus, CancellationToken ct)
    {
        var query = new GetExtractedValueImageQuery(id);
        var result = await bus.InvokeAsync<Result<GetFileDto>>(query, ct);

        if (result.IsFailed)
        {
            if (result.Errors.Any(e => e is NotFoundError))
            {
                return Results.NotFound(result.Errors.Select(e => e.Message));
            }

            return Results.Problem(string.Join("; ", result.Errors.Select(e => e.Message)));
        }

        return result.Value?.Stream is null
            ? Results.NotFound("File not found.")
            : Results.File(result.Value.Stream, result.Value.ContentType, result.Value.FileName);
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
}