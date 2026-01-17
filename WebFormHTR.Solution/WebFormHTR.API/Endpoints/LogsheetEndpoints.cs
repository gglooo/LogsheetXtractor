using FluentResults;
using Microsoft.AspNetCore.Mvc;
using WebFormHTR.API.Extensions;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Features.Logsheets;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Features.ROIs.DTOs;
using Wolverine;
using Wolverine.Http;

namespace WebFormHTR.API.Endpoints;

public sealed record SetLogsheetAlignmentRequest(
    AlignmentDataDto Alignment
);

public static class LogsheetEndpoints
{
    [WolverineGet("/api/logsheets/{id}")]
    [ProducesResponseType(200, Type = typeof(LogsheetDetailDto))]
    [ProducesResponseType(404)]
    public static async Task<IResult> GetLogsheet(
        Guid id,
        IMessageBus bus,
        CancellationToken ct)
    {
        var query = new GetLogsheetQuery(id);
        var result = await bus.InvokeAsync<Result<LogsheetDetailDto>>(query, ct);

        return result.ToHttpResult();
    }

    [WolverinePost("/api/logsheets")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public static async Task<IResult> CreateLogsheet(
        CreateLogsheetCommand request,
        CancellationToken ct,
        IMessageBus bus
    )
    {
        var result = await bus.InvokeAsync<Result<LogsheetDetailDto>>(request, ct);

        return result.ToHttpResult();
    }

    [WolverinePost("/api/logsheets/batch")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public static async Task<IResult> CreateBatchLogsheets(
        BatchCreateLogsheetCommand request,
        CancellationToken ct,
        IMessageBus bus
    )
    {
        var result = await bus.InvokeAsync<Result<IEnumerable<LogsheetDetailDto>>>(request, ct);

        return result.ToHttpResult();
    }

    [WolverinePatch("/api/logsheets/{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public static async Task<IResult> UpdateLogsheet(
        Guid id,
        PatchLogsheetDto request,
        IMessageBus bus,
        CancellationToken ct)
    {
        var command = new PatchLogsheetCommand(id, request);
        var result = await bus.InvokeAsync<Result<LogsheetDetailDto>>(command, ct);

        return result.ToHttpResult();
    }

    [WolverineDelete("/api/logsheets/{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public static async Task<IResult> DeleteLogsheet(
        Guid id,
        IMessageBus bus,
        CancellationToken ct)
    {
        var command = new DeleteLogsheetCommand(id);
        var result = await bus.InvokeAsync<Result>(command, ct);

        return result.ToHttpResult();
    }

    [WolverineDelete("/api/logsheets/batch")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public static async Task<IResult> DeleteBatchLogsheets(
        BatchDeleteLogsheetCommand request,
        IMessageBus bus,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result>(request, ct);

        return result.ToHttpResult();
    }

    [WolverineGet("/api/templates/{id}/logsheets")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<LogsheetListDto>))]
    [ProducesResponseType(404)]
    public static async Task<IResult> ListLogsheetsByTemplate(
        Guid id,
        IMessageBus bus,
        CancellationToken ct)
    {
        var query = new ListLogsheetsByTemplateQuery(id);
        var result = await bus.InvokeAsync<Result<IEnumerable<LogsheetListDto>>>(query, ct);

        return result.ToHttpResult();
    }

    [WolverinePost("/api/logsheets/{id}/process")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public static async Task<IResult> ProcessLogsheetData(
        Guid id,
        IMessageBus bus,
        CancellationToken ct)
    {
        var command = new ProcessLogsheetDataCommand(id);
        var result = await bus.InvokeAsync<Result<LogsheetDetailDto>>(command, ct);

        return result.ToHttpResult();
    }

    [WolverinePost("/api/logsheets/batch/process")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public static async Task<IResult> ProcessBatchLogsheetData(
        BatchProcessLogsheetDataCommand request,
        IMessageBus bus,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<IEnumerable<LogsheetDetailDto>>>(request, ct);

        return result.ToHttpResult();
    }

    [WolverinePost("/api/logsheets/{id}/align")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public static async Task<IResult> AlignLogsheet(
        Guid id,
        IMessageBus bus,
        CancellationToken ct)
    {
        var command = new AlignLogsheetCommand(id);
        var result = await bus.InvokeAsync<Result<LogsheetDetailDto>>(command, ct);

        return result.ToHttpResult();
    }

    [WolverinePost("/api/logsheets/{id}/alignment/set")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public static async Task<IResult> SetLogsheetAlignment(
        Guid id,
        SetLogsheetAlignmentRequest request,
        IMessageBus bus,
        CancellationToken ct)
    {
        var command = new SetLogsheetAlignmentCommand(id, request.Alignment);
        var result = await bus.InvokeAsync<Result<LogsheetDetailDto>>(command, ct);

        return result.ToHttpResult();
    }

    [WolverinePost("/api/logsheets/{id}/proofreading/complete")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public static async Task<IResult> CompleteLogsheetProofreading(
        Guid id,
        IMessageBus bus,
        CancellationToken ct)
    {
        var command = new CompleteLogsheetProofreadingCommand(id);
        var result = await bus.InvokeAsync<Result<LogsheetDetailDto>>(command, ct);

        return result.ToHttpResult();
    }

    [WolverineGet("/api/logsheets/{id}/image")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public static async Task<IResult> GetLogsheetImage(
        Guid id,
        IMessageBus bus,
        CancellationToken ct)
    {
        // TODO: support backside (the method supports it, just parse from request)
        var command = new GetLogsheetImageQuery(id, true);
        var result = await bus.InvokeAsync<Result<GetFileDto>>(command, ct);

        return result.ToHttpResult();
    }

    [WolverineGet("/api/logsheets/{id}/aligned-rois")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public static async Task<IResult> GetAlignedRois(
        Guid id,
        IMessageBus bus,
        CancellationToken ct)
    {
        // TODO: support backside (the method supports it, just parse from request)
        // TODO: this does not work as expected
        var command = new GetAlignedRoisQuery(id, true);
        var result = await bus.InvokeAsync<Result<IEnumerable<RoiDto>>>(command, ct);

        return result.ToHttpResult();
    }
}