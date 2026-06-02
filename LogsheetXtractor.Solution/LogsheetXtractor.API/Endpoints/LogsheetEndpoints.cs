using FluentResults;
using LogsheetXtractor.API.Extensions;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Features.Logsheets;
using LogsheetXtractor.Application.Features.Logsheets.Create;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace LogsheetXtractor.API.Endpoints;

/// <summary>
/// Request payload for setting explicit alignment data on a logsheet.
/// </summary>
/// <param name="Alignment">
/// Alignment coordinates used to project the scanned logsheet onto the template.
/// </param>
public sealed record SetLogsheetAlignmentRequest(AlignmentDataDto Alignment);

/// <summary>
/// Request payload for logsheet processing options.
/// </summary>
/// <param name="Options">
/// Optional extraction flags forwarded to the processing pipeline.
/// </param>
public sealed record ProcessLogsheetDataRequest(ProcessLogsheetDataOptions? Options);

/// <summary>
/// Endpoints for creating, querying, processing, and exporting logsheets.
/// </summary>
public static class LogsheetEndpoints
{
    /// <summary>
    /// Retrieves a single logsheet with template, file, and extracted-value details.
    /// </summary>
    [WolverineGet("/api/logsheets/{id}")]
    [ProducesResponseType(200, Type = typeof(LogsheetDetailDto))]
    [ProducesResponseType(404)]
    public static async Task<IResult> GetLogsheet(Guid id, IMessageBus bus, CancellationToken ct)
    {
        var query = new GetLogsheetQuery(id);
        var result = await bus.InvokeAsync<Result<LogsheetDetailDto>>(query, ct);

        return result.ToHttpResult();
    }

    /// <summary>
    /// Creates a new logsheet for a template and file.
    /// </summary>
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

    /// <summary>
    /// Creates multiple logsheets in a single request.
    /// </summary>
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

    /// <summary>
    /// Applies a partial update to a logsheet.
    /// </summary>
    [WolverinePatch("/api/logsheets/{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public static async Task<IResult> UpdateLogsheet(
        Guid id,
        PatchLogsheetDto request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var command = new PatchLogsheetCommand(id, request);
        var result = await bus.InvokeAsync<Result<LogsheetDetailDto>>(command, ct);

        return result.ToHttpResult();
    }

    /// <summary>
    /// Deletes a logsheet by identifier.
    /// </summary>
    [WolverineDelete("/api/logsheets/{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public static async Task<IResult> DeleteLogsheet(Guid id, IMessageBus bus, CancellationToken ct)
    {
        var command = new DeleteLogsheetCommand(id);
        var result = await bus.InvokeAsync<Result>(command, ct);

        return result.ToHttpResult();
    }

    /// <summary>
    /// Deletes multiple logsheets in a single request.
    /// </summary>
    [WolverineDelete("/api/logsheets/batch")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public static async Task<IResult> DeleteBatchLogsheets(
        BatchDeleteLogsheetCommand request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<Result>(request, ct);

        return result.ToHttpResult();
    }

    /// <summary>
    /// Lists logsheets associated with a template.
    /// </summary>
    [WolverineGet("/api/templates/{id}/logsheets")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<LogsheetListDto>))]
    [ProducesResponseType(404)]
    public static async Task<IResult> ListLogsheetsByTemplate(
        Guid id,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var query = new ListLogsheetsByTemplateQuery(id);
        var result = await bus.InvokeAsync<Result<IEnumerable<LogsheetListDto>>>(query, ct);

        return result.ToHttpResult();
    }

    /// <summary>
    /// Starts asynchronous data processing for a single logsheet.
    /// Returns <c>202 Accepted</c> when the request was queued successfully.
    /// </summary>
    [WolverinePost("/api/logsheets/{id}/process")]
    [ProducesResponseType(202)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public static async Task<IResult> ProcessLogsheetData(
        Guid id,
        ProcessLogsheetDataRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<Result>(
            new StartLogsheetProcessingCommand(id, request.Options),
            ct
        );

        return result.IsSuccess ? Results.Accepted() : result.ToHttpResult();
    }

    /// <summary>
    /// Starts asynchronous data processing for multiple logsheets.
    /// Returns <c>202 Accepted</c> when at least one logsheet was queued successfully.
    /// </summary>
    [WolverinePost("/api/logsheets/batch/process")]
    [ProducesResponseType(202)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public static async Task<IResult> ProcessBatchLogsheetData(
        StartBatchLogsheetProcessingCommand request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<Result>(request, ct);

        return result.IsSuccess ? Results.Accepted() : result.ToHttpResult();
    }

    /// <summary>
    /// Runs automatic alignment for a logsheet.
    /// The result can still be manually adjusted afterwards by setting alignment explicitly.
    /// </summary>
    [WolverinePost("/api/logsheets/{id}/align")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public static async Task<IResult> AlignLogsheet(Guid id, IMessageBus bus, CancellationToken ct)
    {
        var command = new AlignLogsheetCommand(id);
        var result = await bus.InvokeAsync<Result<LogsheetDetailDto>>(command, ct);

        return result.ToHttpResult();
    }

    /// <summary>
    /// Stores explicit alignment data for a logsheet.
    /// Intended for manual correction when automatic alignment is insufficient.
    /// </summary>
    [WolverinePost("/api/logsheets/{id}/alignment/set")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public static async Task<IResult> SetLogsheetAlignment(
        Guid id,
        SetLogsheetAlignmentRequest request,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var command = new SetLogsheetAlignmentCommand(id, request.Alignment);
        var result = await bus.InvokeAsync<Result<LogsheetDetailDto>>(command, ct);

        return result.ToHttpResult();
    }

    /// <summary>
    /// Marks proofreading as complete for a logsheet so its extracted values are finalized.
    /// </summary>
    [WolverinePost("/api/logsheets/{id}/proofreading/complete")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public static async Task<IResult> CompleteLogsheetProofreading(
        Guid id,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var command = new CompleteLogsheetProofreadingCommand(id);
        var result = await bus.InvokeAsync<Result<LogsheetDetailDto>>(command, ct);

        return result.ToHttpResult();
    }

    /// <summary>
    /// Resets proofreading status for a logsheet so extracted values can be reviewed again.
    /// </summary>
    [WolverinePost("/api/logsheets/{id}/proofreading/reset")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public static async Task<IResult> ResetLogsheetProofreading(
        Guid id,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var command = new ResetLogsheetProofreadingCommand(id);
        var result = await bus.InvokeAsync<Result>(command, ct);

        return result.ToHttpResult();
    }

    /// <summary>
    /// Returns a rendered logsheet image for either frontside or backside view.
    /// </summary>
    [WolverineGet("/api/logsheets/{id}/image")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public static async Task<IResult> GetLogsheetImage(
        Guid id,
        [FromQuery] bool backside,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var command = new GetLogsheetImageQuery(id, !backside);
        var result = await bus.InvokeAsync<Result<GetFileDto>>(command, ct);

        return result.ToHttpResult();
    }

    /// <summary>
    /// Exports extracted data for a single logsheet into a downloadable file.
    /// </summary>
    [WolverinePost("/api/logsheets/{id}/export")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public static async Task<IResult> ExportLogsheetData(
        Guid id,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var command = new ExportLogsheetDataCommand(id);
        var result = await bus.InvokeAsync<Result<GetFileDto>>(command, ct);

        return result.ToHttpResult();
    }

    /// <summary>
    /// Exports extracted data for a batch of logsheets into a downloadable file.
    /// </summary>
    [WolverinePost("/api/logsheets/batch/export")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public static async Task<IResult> ExportBatchLogsheetData(
        BatchExportLogsheetDataCommand command,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<Result<GetFileDto>>(command, ct);

        return result.ToHttpResult();
    }
}
