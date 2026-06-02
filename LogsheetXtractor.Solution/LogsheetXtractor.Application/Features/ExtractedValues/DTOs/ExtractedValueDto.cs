using LogsheetXtractor.Application.Features.RoiValidation.DTOs;
using LogsheetXtractor.Domain.Enums;

namespace LogsheetXtractor.Application.Features.ExtractedValues.DTOs;

/// <summary>
/// Represents one extracted ROI value, including verification and validation metadata.
/// </summary>
public record ExtractedValueDto(
    Guid Id,
    Guid LogsheetId,
    Guid RoiId,
    ERoiType RoiType,
    string VariableName,
    string Value,
    string? CorrectedValue,
    EVerificationStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<RoiValidationWarningDto>? ValidationWarnings = null,
    string? ValidationRulesVersion = null
);
