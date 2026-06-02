using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Domain.ValueObjects;
using LogsheetXtractor.Domain.ValueObjects.RoiValidation;

namespace LogsheetXtractor.Application.Features.ROIs.DTOs;

/// <summary>
/// ROI definition persisted on a template for extraction and proofreading.
/// </summary>
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
