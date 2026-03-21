using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Domain.ValueObjects;
using LogsheetXtractor.Domain.ValueObjects.RoiValidation;

namespace LogsheetXtractor.Application.Features.ROIs.DTOs;

public record SetRoiDto(
    string? Id,
    string VariableName,
    ERoiType? Type,
    Coordinates Coordinates,
    RoiValidationConditionNode? ValidationCondition = null
);
