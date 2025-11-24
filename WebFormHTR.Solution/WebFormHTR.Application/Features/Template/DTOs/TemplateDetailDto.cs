using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Application.Features.ROIs.DTOs;

namespace WebFormHTR.Application.Features.Template.DTOs;

public record TemplateDetailDto
(
    Guid Id,
    string Name,
    TemplateWithoutParentDto? Parent,
    FileDto? File,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IEnumerable<RoiDto> Rois
// TODO: add logsheets, etc
);