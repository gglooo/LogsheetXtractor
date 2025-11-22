using Microsoft.AspNetCore.Mvc;
using WebFormHTR.Application.Features.Template;
using WebFormHTR.Domain.Entities;
using Wolverine;
using Wolverine.Http;

namespace WebFormHTR.API.Endpoints;

public static class TemplateEndpoints
{
    [WolverineGet("/api/templates/{id}")]
    [ProducesResponseType(200, Type = typeof(Template))]
    [ProducesResponseType(404)]
    public static async Task<IResult> GetTemplate(
        string id, 
        IMessageBus bus,
        CancellationToken ct)
    {
        var query = new GetTemplateQuery(id);
        
        var res = await bus.InvokeAsync<Template?>(query, ct);
        
        return res is null ? Results.NotFound() : Results.Ok(res);
    }
    
    [WolverineGet("/api/templates")] 
    [ProducesResponseType(200, Type = typeof(IEnumerable<Template>))]
    public static async Task<IResult> ListTemplates(
        string? search, 
        IMessageBus bus,
        CancellationToken ct)
    {
        var query = new ListTemplatesQuery(search);
        
        return Results.Ok(await bus.InvokeAsync<IEnumerable<Template>>(query, ct));
    }
}