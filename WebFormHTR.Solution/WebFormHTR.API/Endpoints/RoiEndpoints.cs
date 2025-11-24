using FluentResults;
using Microsoft.AspNetCore.Mvc;
using WebFormHTR.API.Extensions;
using WebFormHTR.Application.Features.ROIs;
using WebFormHTR.Application.Features.ROIs.DTOs;
using Wolverine;
using Wolverine.Http;

namespace WebFormHTR.API.Endpoints;

public sealed record SetRoiRequest(
    IEnumerable<SetRoiDto> Rois
);

public static class RoiEndpoints
{
    [WolverinePost("/api/templates/{id}/rois")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public static async Task<IResult> SetRoisForTemplate(
        Guid id,
        SetRoiRequest request,
        CancellationToken ct,
        IMessageBus bus
    )
    {
        var command = new SetTemplateRoisCommand(id, request.Rois);

        var result = await bus.InvokeAsync<Result<IEnumerable<RoiDto>>>(command, ct);
        return result.ToHttpResult();
    }

    [WolverineGet("/api/templates/{id}/rois")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public static async Task<IResult> GetRoisForTemplate(
        Guid id,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var query = new ListRoisForTemplateQuery(id);

        var result = await bus.InvokeAsync<Result<IEnumerable<RoiDto>>>(query, ct);

        return result.ToHttpResult();
    }
}