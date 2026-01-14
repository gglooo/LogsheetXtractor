using FluentResults;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.File.Interfaces;

namespace WebFormHTR.API.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsFailed)
        {
            if (result.Errors.Any(e => e is NotFoundError))
            {
                return Results.NotFound(result.Errors.Select(e => e.Message));
            }

            return Results.BadRequest(result.Errors.Select(e => e.Message));
        }

        if (result.Value is null)
        {
            return Results.NotFound();
        }

        if (result.Value is IFileResponse fileResponse)
        {
            return Results.File(
                fileResponse.Stream,
                fileResponse.ContentType,
                fileResponse.FileName
            );
        }

        return Results.Ok(result.Value);
    }

    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok();
        }

        if (result.Errors.Any(e => e is NotFoundError))
        {
            return Results.NotFound(result.Errors.Select(e => e.Message));
        }

        return Results.BadRequest(result.Errors.Select(e => e.Message));
    }
}