using FluentResults;
using LogsheetXtractor.API.Extensions;
using LogsheetXtractor.Application.Features.Residuals;
using LogsheetXtractor.Application.Features.Residuals.DTOs;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace LogsheetXtractor.API.Endpoints;

public sealed record CreateResidualRequest(IEnumerable<CreateResidualDto> Residuals);

public sealed record SetResidualRequest(IEnumerable<SetResidualDto> Residuals);

public sealed record UpsertResidualRequest(UpsertResidualDto Residual);

public static class ResidualEndpoints
{
    [WolverineGet("/api/templates/{templateId}/residuals")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public static async Task<IResult> GetResidualsForTemplate(
        Guid templateId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var query = new ListResidualsForTemplateQuery(templateId);
        var result = await bus.InvokeAsync<Result<IEnumerable<ResidualDto>>>(query, ct);

        return result.ToHttpResult();
    }

    [WolverinePost("/api/templates/{templateId}/residuals")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public static async Task<IResult> CreateResiduals(
        Guid templateId,
        CreateResidualRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var command = new CreateResidualCommand(templateId, request.Residuals);
        var result = await bus.InvokeAsync<Result<IEnumerable<ResidualDto>>>(command, ct);

        return result.ToHttpResult();
    }

    [WolverinePost("/api/templates/{templateId}/residuals/set")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public static async Task<IResult> SetResidualsForTemplate(
        Guid templateId,
        SetResidualRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var command = new SetTemplateResidualsCommand(templateId, request.Residuals);
        var result = await bus.InvokeAsync<Result<IEnumerable<ResidualDto>>>(command, ct);

        return result.ToHttpResult();
    }

    [WolverinePost("/api/templates/{templateId}/residuals/upsert")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public static async Task<IResult> UpsertResidualForTemplate(
        Guid templateId,
        UpsertResidualRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var command = new UpsertResidualCommand(templateId, request.Residual);
        var result = await bus.InvokeAsync<Result<ResidualDto>>(command, ct);

        return result.ToHttpResult();
    }
}
