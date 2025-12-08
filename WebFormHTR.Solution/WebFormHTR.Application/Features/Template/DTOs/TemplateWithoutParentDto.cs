using WebFormHTR.Application.Features.File.DTOs;

namespace WebFormHTR.Application.Features.Template.DTOs;

public record TemplateWithoutParentDto(
    Guid Id,
    string Name,
    float Width,
    float Height,
    FileDto? File,
    DateTime CreatedAt,
    DateTime UpdatedAt
);