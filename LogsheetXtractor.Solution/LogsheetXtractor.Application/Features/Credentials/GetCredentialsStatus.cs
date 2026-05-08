using System.Text.Json;
using FluentResults;
using LogsheetXtractor.Application.Features.Credentials.DTOs;
using LogsheetXtractor.Application.Interfaces;

namespace LogsheetXtractor.Application.Features.Credentials;

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