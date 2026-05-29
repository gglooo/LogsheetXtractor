namespace LogsheetXtractor.Application.Features.File.DTOs;

public record FileDto
(
    Guid Id,
    string FileName,
    string ContentType,
    uint SizeBytes,
    DateTime CreatedAt
);