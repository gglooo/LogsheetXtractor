using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Features.File.DTOs;

namespace WebFormHTR.Application.Features.Template.DTOs;

public record TemplateDetailDto
(
    Guid Id,
    string Name,
    TemplateDetailDto? Parent,
    FileDto? File,
    DateTime CreatedAt,
    DateTime UpdatedAt
    
    // TODO: add logsheets, rois etc
);