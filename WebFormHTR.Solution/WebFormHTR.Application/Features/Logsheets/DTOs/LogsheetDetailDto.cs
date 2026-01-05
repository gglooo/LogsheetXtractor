using WebFormHTR.Application.Features.ExtractedValues.DTOs;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.Logsheets.DTOs;

public record LogsheetDetailDto(
    Guid Id,
    TemplateListDto Template,
    TemplateListDto? BacksideTemplate,
    FileDto File,
    ELogSheetStatus Status,
    DateTime? ProcessedAt,
    AlignmentDataDto? AlignmentData,
    IEnumerable<ExtractedValueDto> ExtractedValues,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);