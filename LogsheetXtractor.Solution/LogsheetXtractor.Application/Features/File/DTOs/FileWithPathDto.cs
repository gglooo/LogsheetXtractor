namespace LogsheetXtractor.Application.Features.File.DTOs;

public record FileWithPathDto(
    Guid Id,
    string FileName,
    string FilePath,
    string ContentType,
    uint SizeBytes,
    DateTime CreatedAt
);