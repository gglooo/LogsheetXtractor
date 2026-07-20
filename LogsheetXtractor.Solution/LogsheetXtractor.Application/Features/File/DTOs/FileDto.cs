namespace LogsheetXtractor.Application.Features.File.DTOs;

/// <summary>
/// File metadata returned by application services without opening the file content.
/// </summary>
public record FileDto
(
    Guid Id,
    string FileName,
    string ContentType,
    uint SizeBytes,
    DateTime CreatedAt
);
