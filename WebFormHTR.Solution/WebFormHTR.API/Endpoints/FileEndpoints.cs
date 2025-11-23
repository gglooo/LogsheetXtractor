using FluentResults;
using Microsoft.AspNetCore.Mvc;
using WebFormHTR.API.Extensions;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.File.DTOs;
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
       
        var result = await bus.InvokeAsync<Result<FileDto>>(command, ct);

        return result.ToHttpResult();
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
        var result = await bus.InvokeAsync<Result<GetFileDto?>>(query, ct);

        if (result.IsFailed)
        {
            if (result.Errors.Any(e => e is NotFoundError))
            {
                return Results.NotFound(result.Errors.Select(e => e.Message));
            }
            
            return Results.Problem(string.Join("; ", result.Errors.Select(e => e.Message)));
        }
        
        return result.Value?.Stream is null ? Results.NotFound("File not found.") : Results.File(result.Value.Stream, result.Value.ContentType, result.Value.FileName);
    }
}