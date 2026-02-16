
namespace WebFormHTR.Application.Features.Template.DTOs;

public record TemplateReferenceDto(
    Guid Id,
    string Name,
    int Width,
    int Height,
    Guid FileId
);
