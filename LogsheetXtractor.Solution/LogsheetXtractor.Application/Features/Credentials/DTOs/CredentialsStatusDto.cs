namespace LogsheetXtractor.Application.Features.Credentials.DTOs;

/// <summary>
/// Indicates whether OCR credential sources are available for processing.
/// </summary>
public sealed record CredentialsStatusDto(
    bool Available,
    bool HasUserCredentials
);
