using System.Text.Json;
using FluentResults;
using LogsheetXtractor.API.Extensions;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Features.Credentials.DTOs;
using LogsheetXtractor.Application.Features.Credentials.SetUserCredentials;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace LogsheetXtractor.API.Endpoints;

public sealed record SetUserCredentialsRequest(Dictionary<ECredentialType, string> Keys);

public static class CredentialsEndpoints
{
    [WolverineGet("/api/credentials/status")]
    [ProducesResponseType(200, Type = typeof(CredentialsStatusDto))]
    public static async Task<IResult> GetCredentialsStatus(
        IMessageBus bus,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<Result<CredentialsStatusDto?>>(
            new GetCredentialsStatusQuery(
                httpContext.Request.Cookies[CredentialsConstants.CookieName]
            ),
            ct
        );

        if (result.IsFailed)
        {
            return result.ToHttpResult();
        }

        return result.Value is null ? Results.NoContent() : Results.Ok(result.Value);
    }

    [WolverinePost("/api/credentials")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public static async Task<IResult> SetUserCredentials(
        [FromBody] SetUserCredentialsRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var command = new SetUserCredentialsCommand(request.Keys);
        var result = await bus.InvokeAsync<Result>(command, ct);

        if (!result.IsSuccess)
        {
            return result.ToHttpResult();
        }

        var options = new CookieOptions
        {
            HttpOnly = true,
            // TODO: will the app always run behind HTTP?
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddYears(1),
        };

        var keysJson = JsonSerializer.Serialize(request.Keys);
        httpContext.Response.Cookies.Append(CredentialsConstants.CookieName, keysJson, options);

        return result.ToHttpResult();
    }

    [WolverineDelete("/api/credentials")]
    [ProducesResponseType(204)]
    public static async Task<IResult> DeleteUserCredentials(
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var command = new DeleteUserCredentialsCommand();
        var result = await bus.InvokeAsync<Result>(command, ct);

        if (result.IsSuccess)
        {
            httpContext.Response.Cookies.Delete(CredentialsConstants.CookieName);
        }

        return result.ToHttpResult();
    }
}
