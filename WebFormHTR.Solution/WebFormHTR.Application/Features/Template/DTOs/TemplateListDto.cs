namespace WebFormHTR.Application.Features.Template.DTOs;

public record TemplateListDto(
    Guid Id,
    string Name,
    Guid? BacksideTemplateId,
    Guid? ParentId,
    Guid? FileId,
    int RoiCount,
    int LogsheetCount,
    int Width,
    int Height,
    DateTime CreatedAt
);