using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.ExtractedValues.DTOs;

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
    DateTime? UpdatedAt
);