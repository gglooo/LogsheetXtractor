using WebFormHTR.Domain.Enums;
using WebFormHTR.Domain.ValueObjects;
using WebFormHTR.Domain.ValueObjects.RoiValidation;

namespace WebFormHTR.Application.Features.ROIs.DTOs;

public record RoiDto(
    Guid? Id,
    string VariableName,
    Guid TemplateId,
    ERoiType? Type,
    Coordinates Coordinates,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    RoiValidationConditionNode? ValidationCondition = null
);
