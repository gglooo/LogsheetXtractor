namespace LogsheetXtractor.Application.Features.Credentials.DTOs;

public sealed record CredentialsStatusDto(
    bool Available,
    bool HasUserCredentials
);