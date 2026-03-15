using FluentResults;
using Microsoft.AspNetCore.Mvc;
using WebFormHTR.API.Extensions;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Application.Features.Template;
using WebFormHTR.Application.Features.Template.CreateTemplate;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Domain.Entities;
using Wolverine;
using Wolverine.Http;

namespace WebFormHTR.API.Endpoints;

public sealed record CloneTemplateBacksideRequest(string Name, Guid FileId);

public sealed record CloneTemplateRequest(string NewName, Guid FileId, CloneTemplateBacksideRequest? Backside);
public sealed record AddTemplateBacksideRequest(string Name, Guid FileId);

public static class TemplateEndpoints
{
    [WolverineGet("/api/templates/{id}")]
    [ProducesResponseType(200, Type = typeof(TemplateDetailDto))]
    [ProducesResponseType(404)]
    public static async Task<IResult> GetTemplate(
        Guid id,
        IMessageBus bus,
        CancellationToken ct)
    {
        var query = new GetTemplateQuery(id);
        var result = await bus.InvokeAsync<Result<TemplateDetailDto?>>(query, ct);

        return result.ToHttpResult();
    }

    [WolverineGet("/api/templates")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<TemplateListDto>))]
    public static async Task<IResult> ListTemplates(
        string? search,
        IMessageBus bus,
        CancellationToken ct)
    {
        var query = new ListTemplatesQuery(search);

        var result = await bus.InvokeAsync<Result<IEnumerable<TemplateListDto>>>(query, ct);

        return result.ToHttpResult();
    }

    [WolverinePost("/api/templates")]
    [ProducesResponseType(200, Type = typeof(TemplateDetailDto))]
    [ProducesResponseType(400)]
    public static async Task<IResult> CreateTemplate(
        CreateTemplateCommand command,
        IMessageBus bus,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Results.BadRequest("Template name is required.");
        }

        var result = await bus.InvokeAsync<Result<TemplateDetailDto>>(command, ct);

        return result.ToHttpResult();
    }

    [WolverineDelete("/api/templates/{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public static async Task<IResult> DeleteTemplate(
        Guid id,
        IMessageBus bus,
        CancellationToken ct)
    {
        var command = new DeleteTemplateCommand(id);
        var result = await bus.InvokeAsync<Result>(command, ct);

        return result.ToHttpResult();
    }

    [WolverinePost("/api/templates/{id}/clone")]
    [ProducesResponseType(200, Type = typeof(TemplateDetailDto))]
    [ProducesResponseType(400)]
    public static async Task<IResult> CloneTemplate(
        Guid id,
        CloneTemplateRequest request,
        IMessageBus bus,
        CancellationToken ct)
    {
        var backsideCommand = request.Backside != null
            ? new CloneTemplateBacksideCommand(request.Backside.Name, request.Backside.FileId)
            : null;
        var command = new CloneTemplateCommand(id, request.NewName, request.FileId, backsideCommand);

        var result = await bus.InvokeAsync<Result<TemplateDetailDto>>(command, ct);

        return result.ToHttpResult();
    }

    [WolverinePost("/api/templates/{id}/backside")]
    [ProducesResponseType(200, Type = typeof(TemplateDetailDto))]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public static async Task<IResult> AddTemplateBackside(
        Guid id,
        AddTemplateBacksideRequest request,
        IMessageBus bus,
        CancellationToken ct)
    {
        var command = new AddTemplateBacksideCommand(id, request.Name, request.FileId);
        var result = await bus.InvokeAsync<Result<TemplateDetailDto>>(command, ct);

        return result.ToHttpResult();
    }

    [WolverinePost("/api/templates/{id}/detect-rois")]
    [ProducesResponseType(200, Type = typeof(DetectRoisResponseDto))]
    [ProducesResponseType(400)]
    public static async Task<IResult> DetectRois(
        Guid id,
        IMessageBus bus,
        CancellationToken ct)
    {
        var command = new DetectRoisCommand(id);
        var result = await bus.InvokeAsync<Result<DetectRoisResponseDto>>(command, ct);

        return result.ToHttpResult();
    }

    [WolverinePost("/api/templates/{id}/export-config")]
    [ProducesResponseType(200, Type = typeof(GetFileDto))]
    [ProducesResponseType(404)]
    public static async Task<IResult> ExportTemplateConfig(
        Guid id,
        IMessageBus bus,
        CancellationToken ct)
    {
        var query = new ExportTemplateConfigQuery(id);
        var result = await bus.InvokeAsync<Result<GetFileDto>>(query, ct);

        return result.ToHttpResult();
    }

    [WolverinePost("/api/templates/identify-from-file/{fileId}")]
    [ProducesResponseType(200, Type = typeof(GetFileDto))]
    [ProducesResponseType(404)]
    public static async Task<IResult> IdentifyFromFile(
        Guid fileId,
        IMessageBus bus,
        CancellationToken ct)
    {
        var query = new IdentifyTemplatesFromFileQuery(fileId);
        var result = await bus.InvokeAsync<Result<Dictionary<int, TemplateListDto>>>(query, ct);

        return result.ToHttpResult();
    }
    
    [WolverineGet("/api/templates/{id}/preview")]
    [ProducesResponseType(200, Type = typeof(GetFileDto))]
    [ProducesResponseType(404)]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public static async Task<IResult> GetTemplatePreview(
        Guid id,
        IMessageBus bus,
        CancellationToken ct)
    {
        var query = new GetTemplatePreviewQuery(id);
        var result = await bus.InvokeAsync<Result<GetFileDto>>(query, ct);

        return result.ToHttpResult();
    }
}
