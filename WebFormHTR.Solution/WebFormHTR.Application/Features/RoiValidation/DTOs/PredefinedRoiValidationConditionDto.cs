using WebFormHTR.Domain.Enums;
using WebFormHTR.Domain.ValueObjects.RoiValidation;

namespace WebFormHTR.Application.Features.RoiValidation.DTOs;

public sealed record PredefinedRoiValidationConditionDto(
    Guid Id,
    string Code,
    string Label,
    ERoiType RoiType,
    RoiValidationConditionNode Condition
);
