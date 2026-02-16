using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Application.Features.Residuals.DTOs;
using WebFormHTR.Application.Features.ROIs.DTOs;

namespace WebFormHTR.Application.Features.Template.DTOs;

public record TemplateDetailDto(
    Guid Id,
    string Name,
    int Width,
    int Height,
    TemplateWithoutParentDto? Parent,
    TemplateReferenceDto? BacksideTemplate,
    TemplateReferenceDto? FrontsideTemplate,
    FileDto? File,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IEnumerable<RoiDto> Rois,
    IEnumerable<ResidualDto> Residuals
);