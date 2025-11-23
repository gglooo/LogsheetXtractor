namespace WebFormHTR.Application.Features.Template.DTOs;

public record TemplateListDto
(
    string Id,
    string Name,
    Guid? ParentId,
    Guid? FileId
);