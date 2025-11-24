using FluentResults;
using WebFormHTR.Application.Errors;

namespace WebFormHTR.API.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result is { IsSuccess: true, Value: null })
        {
            return Results.NotFound();
        }

        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        if (result.Errors.Any(e => e is NotFoundError))
        {
            return Results.NotFound(result.Errors.Select(e => e.Message));
        }

        return Results.BadRequest(result.Errors.Select(e => e.Message));
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