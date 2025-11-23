using FluentResults;
using Microsoft.AspNetCore.Mvc;
using WebFormHTR.API.Extensions;
using WebFormHTR.Application.Features.Template;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Domain.Entities;
using Wolverine;
using Wolverine.Http;

namespace WebFormHTR.API.Endpoints;

public sealed record CreateTemplateRequest(string NewName, Guid? FileId);

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

    [WolverineDelete(("/api/templates/{id}"))]
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
        CreateTemplateRequest request,
        IMessageBus bus,
        CancellationToken ct)
    {
        var command = new CloneTemplateCommand(id, request.NewName, request.FileId);
        
        var result = await bus.InvokeAsync<Result<TemplateDetailDto>>(command, ct);

        return result.ToHttpResult();
    }
}