using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.Logsheets.DTOs;

public record LogsheetDetailDto
(
    Guid Id,
    TemplateListDto Template,
    FileDto File,
    ELogSheetStatus Status,
    DateTime? ProcessedAt
    // TODO: add extracted values list if needed
);