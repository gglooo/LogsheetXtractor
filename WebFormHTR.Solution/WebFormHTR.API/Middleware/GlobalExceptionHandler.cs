using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WebFormHTR.API.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            logger.LogWarning(
                "Validation failed for {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            var errors = validationException.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).ToArray()
                );

            var errorMessages = validationException.Errors
                .Select(x => x.ErrorMessage)
                .ToArray();
            var validationProblemDetails = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Detail = "One or more validation errors occurred."
            };

            validationProblemDetails.Extensions["code"] = "VALIDATION_ERROR";
            validationProblemDetails.Extensions["errorMessages"] = errorMessages;

            httpContext.Response.StatusCode = validationProblemDetails.Status.Value;

            await httpContext.Response
                .WriteAsJsonAsync(validationProblemDetails, cancellationToken);

            return true;
        }

        logger.LogError(
            exception,
            "An unhandled exception occurred processing {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred while processing your request.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}