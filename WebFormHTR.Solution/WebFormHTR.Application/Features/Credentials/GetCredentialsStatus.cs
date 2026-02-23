using System.Text.Json;
using FluentResults;
using WebFormHTR.Application.Features.Credentials.DTOs;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Credentials;

public sealed record GetCredentialsStatusQuery(string? UserCredentials);

public static class GetCredentialsStatusHandler
{
    public static async Task<Result<CredentialsStatusDto?>> Handle(GetCredentialsStatusQuery query,
        ICredentialService credentialsService,
        CancellationToken ct)
    {
        var credentials = await credentialsService.GetAvailableCredentialTypesAsync(ct);

        var keys = CredentialCookieParser.ParseCredentials(query.UserCredentials);
        var hasUserCredentials = keys != null;

        return new CredentialsStatusDto(credentials.Any(), hasUserCredentials);
    }
}