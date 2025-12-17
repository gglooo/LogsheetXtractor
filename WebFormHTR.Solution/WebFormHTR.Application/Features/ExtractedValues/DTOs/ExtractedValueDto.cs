using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.ExtractedValues.DTOs;

public record ExtractedValueDto(
    Guid Id,
    Guid LogsheetId,
    Guid RoiId,
    string VariableName,
    string Value,
    string? CorrectedValue,
    EVerificationStatus Status
);