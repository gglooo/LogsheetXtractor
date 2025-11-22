using Microsoft.AspNetCore.Mvc;
using WebFormHTR.Application.DTOs;
using Wolverine;
using Wolverine.Http;

namespace WebFormHTR.API.Endpoints;

public static class FileEndpoints
{
    [WolverinePost("/api/files/upload")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public static async Task<IResult> UploadFile(IFormFile formFile, IMessageBus bus, CancellationToken ct)
    {
       if (formFile.Length == 0)
       {
           return Results.BadRequest("No file uploaded.");
       }
       
       using var ms = new MemoryStream();
       await formFile.CopyToAsync(ms, ct);
        
       var command = new Application.Features.File.UploadFileCommand(
           ms.ToArray(),
           formFile.FileName,
           formFile.ContentType
       );
       
        var file = await bus.InvokeAsync<Domain.Entities.File>(command, ct);
        
        return Results.Ok(file);
    }

    [WolverineGet("/api/files/{id}")]
    [ProducesResponseType(200, Type = typeof(GetFileDto))]
    [ProducesResponseType(404)]
    public static async Task<IResult> GetFile(
        string id,
        IMessageBus bus,
        CancellationToken ct)
    {
        var query = new Application.Features.File.GetFileQuery(id);
        var res = await bus.InvokeAsync<GetFileDto?>(query, ct);

        return res?.Stream is null ? Results.NotFound() : Results.File(res.Stream, res.ContentType, res.FileName);
    }
}