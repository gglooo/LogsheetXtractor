namespace LogsheetXtractor.Application.Features.File.DTOs;

/// <summary>
/// TODO-DOC: Describe FileDto purpose and usage.
/// </summary>
public record FileDto
(
    Guid Id,
    string FileName,
    string ContentType,
    uint SizeBytes,
    DateTime CreatedAt
);
