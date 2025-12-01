using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.Logsheets.DTOs;

public record LogsheetDetailDto(
    Guid Id,
    TemplateListDto Template,
    TemplateListDto BacksideTemplate,
    FileDto File,
    ELogSheetStatus Status,
    DateTime? ProcessedAt,
    AlignmentDataDto? AlignmentData
// TODO: add extracted values list if needed
);