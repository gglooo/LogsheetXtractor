using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Domain.ValueObjects.RoiValidation;

namespace LogsheetXtractor.Application.Features.RoiValidation.DTOs;

public sealed record PredefinedRoiValidationConditionDto(
    Guid Id,
    string Code,
    string Label,
    ERoiType RoiType,
    RoiValidationConditionNode Condition
);
