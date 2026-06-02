using FluentResults;
using LogsheetXtractor.API.Extensions;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Features.File;
using LogsheetXtractor.Application.Features.File.DTOs;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace LogsheetXtractor.API.Endpoints;

/// <summary>
/// Endpoints for file upload and retrieval operations.
/// </summary>
public static class FileEndpoints
{
    /// <summary>
    /// Uploads a file to storage and persists its metadata.
    /// </summary>
    /// <param name="formFile">The multipart form file payload.</param>
    /// <param name="bus">Message bus used to dispatch the upload command.</param>
    /// <param name="ct">Cancellation token for the request.</param>
    /// <returns>
    /// A file DTO when upload succeeds, or a validation error when the payload is invalid.
    /// </returns>
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

    /// <summary>
    /// Returns metadata and content details for a stored file by identifier.
    /// </summary>
    /// <param name="id">The file identifier.</param>
    /// <param name="bus">Message bus used to dispatch the query.</param>
    /// <param name="ct">Cancellation token for the request.</param>
    /// <returns>The file payload when found; otherwise a not found response.</returns>
    [WolverineGet("/api/files/{id}")]
    [ProducesResponseType(200, Type = typeof(GetFileDto))]
    [ProducesResponseType(404)]
    public static async Task<IResult> GetFile(Guid id, IMessageBus bus, CancellationToken ct)
    {
        var query = new GetFileQuery(id);
        var result = await bus.InvokeAsync<Result<GetFileDto?>>(query, ct);

        return result.ToHttpResult();
    }

    /// <summary>
    /// Returns image-ready file content by identifier.
    /// </summary>
    /// <param name="id">The file identifier.</param>
    /// <param name="bus">Message bus used to dispatch the query.</param>
    /// <param name="ct">Cancellation token for the request.</param>
    /// <returns>The image payload when found; otherwise a not found response.</returns>
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
