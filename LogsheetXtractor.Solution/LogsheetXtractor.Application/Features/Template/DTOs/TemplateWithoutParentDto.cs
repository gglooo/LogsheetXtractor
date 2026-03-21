using LogsheetXtractor.Application.Features.File.DTOs;

namespace LogsheetXtractor.Application.Features.Template.DTOs;

public record TemplateWithoutParentDto(
    Guid Id,
    string Name,
    int Width,
    int Height,
    FileDto? File,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
