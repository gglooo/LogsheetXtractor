using FluentResults;
using LogsheetXtractor.API.Extensions;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Features.File;
using LogsheetXtractor.Application.Features.File.DTOs;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace LogsheetXtractor.API.Endpoints;

public static class FileEndpoints
{
    [WolverinePost("/api/files/upload")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public static async Task<IResult> UploadFile(
        IFormFile formFile,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        if (formFile.Length == 0)
        {
            return Results.BadRequest("No file uploaded.");
        }

        using var ms = new MemoryStream();
        await formFile.CopyToAsync(ms, ct);

        var command = new UploadFileCommand(ms.ToArray(), formFile.FileName, formFile.ContentType);

        var result = await bus.InvokeAsync<Result<FileDto>>(command, ct);

        return result.ToHttpResult();
    }

    [WolverineGet("/api/files/{id}")]
    [ProducesResponseType(200, Type = typeof(GetFileDto))]
    [ProducesResponseType(404)]
    public static async Task<IResult> GetFile(Guid id, IMessageBus bus, CancellationToken ct)
    {
        var query = new GetFileQuery(id);
        var result = await bus.InvokeAsync<Result<GetFileDto?>>(query, ct);

        return result.ToHttpResult();
    }

    [WolverineGet("/api/files/{id}/image")]
    [ProducesResponseType(200, Type = typeof(GetFileDto))]
    [ProducesResponseType(404)]
    public static async Task<IResult> GetFileImage(Guid id, IMessageBus bus, CancellationToken ct)
    {
        var query = new GetFileImageQuery(id);
        var result = await bus.InvokeAsync<Result<GetFileDto?>>(query, ct);

        return result.ToHttpResult();
    }
}
