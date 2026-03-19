using WebFormHTR.Domain.Enums;
using WebFormHTR.Domain.ValueObjects;
using WebFormHTR.Domain.ValueObjects.RoiValidation;

namespace WebFormHTR.Application.Features.ROIs.DTOs;

public record UpsertRoiDto
(
    Guid? Id,
    string VariableName,
    ERoiType Type,
    Coordinates Coordinates,
    RoiValidationConditionNode? ValidationCondition = null
);
