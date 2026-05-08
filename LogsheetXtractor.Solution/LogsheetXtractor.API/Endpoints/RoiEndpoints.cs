using FluentResults;
using LogsheetXtractor.API.Extensions;
using LogsheetXtractor.Application.Features.ROIs;
using LogsheetXtractor.Application.Features.ROIs.DTOs;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace LogsheetXtractor.API.Endpoints;

public sealed record SetRoiRequest(IEnumerable<SetRoiDto> Rois);

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
}
