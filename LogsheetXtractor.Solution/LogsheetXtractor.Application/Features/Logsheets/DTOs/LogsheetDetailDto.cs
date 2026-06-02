using LogsheetXtractor.Application.Features.ExtractedValues.DTOs;
using LogsheetXtractor.Application.Features.File.DTOs;
using LogsheetXtractor.Application.Features.Template.DTOs;
using LogsheetXtractor.Domain.Enums;

namespace LogsheetXtractor.Application.Features.Logsheets.DTOs;

/// <summary>
/// Detailed logsheet response used by the processing and proofreading workflows.
/// </summary>
public record LogsheetDetailDto(
    Guid Id,
    TemplateListDto Template,
    FileDto File,
    ELogSheetStatus Status,
    DateTime? ProcessedAt,
    AlignmentDataDto? AlignmentData,
    IEnumerable<ExtractedValueDto> ExtractedValues,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
