namespace LogsheetXtractor.Application.Features.Template.DTOs;

/// <summary>
/// Lightweight template item used in template list views.
/// </summary>
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
