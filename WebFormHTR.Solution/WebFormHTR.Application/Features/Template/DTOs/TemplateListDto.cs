namespace WebFormHTR.Application.Features.Template.DTOs;

public record TemplateListDto(
    Guid Id,
    string Name,
    Guid? ParentId,
    Guid? FileId,
    int RoiCount,
    int Width,
    int Height,
    DateTime CreatedAt
);