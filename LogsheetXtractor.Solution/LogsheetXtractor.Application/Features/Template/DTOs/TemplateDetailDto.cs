using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Features.File.DTOs;
using LogsheetXtractor.Application.Features.Residuals.DTOs;
using LogsheetXtractor.Application.Features.ROIs.DTOs;

namespace LogsheetXtractor.Application.Features.Template.DTOs;

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
    IEnumerable<ResidualDto> Residuals,
    bool IsEditable
);
