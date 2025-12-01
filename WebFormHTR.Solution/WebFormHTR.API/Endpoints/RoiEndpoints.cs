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

public sealed record UpsertRoiRequest(
    UpsertRoiDto Roi
);

public sealed record CreateRoisRequest(
    IEnumerable<CreateRoiDto> Rois
);

public static class RoiEndpoints
{
    [WolverinePost("/api/templates/{templateId}/rois/set")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public static async Task<IResult> SetRoisForTemplate(
        Guid templateId,
        SetRoiRequest request,
        CancellationToken ct,
        IMessageBus bus
    )
    {
        var command = new SetTemplateRoisCommand(templateId, request.Rois);

        var result = await bus.InvokeAsync<Result<IEnumerable<RoiDto>>>(command, ct);
        return result.ToHttpResult();
    }

    [WolverineGet("/api/templates/{templateId}/rois")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public static async Task<IResult> GetRoisForTemplate(
        Guid templateId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var query = new ListRoisForTemplateQuery(templateId);

        var result = await bus.InvokeAsync<Result<IEnumerable<RoiDto>>>(query, ct);

        return result.ToHttpResult();
    }

    [WolverinePost("/api/templates/{templateId}/rois/upsert")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public static async Task<IResult> UpsertRoiForTemplate(
        Guid templateId,
        UpsertRoiRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var command = new UpsertRoiCommand(templateId, request.Roi);

        var result = await bus.InvokeAsync<Result<RoiDto>>(command, ct);
        return result.ToHttpResult();
    }

    [WolverinePost("/api/templates/{templateId}/rois")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public static async Task<IResult> CreateRois(
        Guid templateId,
        CreateRoisRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var command = new CreateRoisCommand(templateId, request.Rois);
        var result = await bus.InvokeAsync<Result>(command, ct);

        return result.ToHttpResult();
    }
}