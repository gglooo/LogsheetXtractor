using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Domain.ValueObjects.RoiValidation;

namespace LogsheetXtractor.Application.Features.RoiValidation.DTOs;

/// <summary>
/// A reusable ROI validation condition offered by the application.
/// <param name="Id">The identifier of the predefined condition.</param>
/// <param name="Code">The stable code used to identify the condition.</param>
/// <param name="Label">The human-readable condition label.</param>
/// <param name="RoiType">The ROI type to which the condition applies.</param>
/// <param name="Condition">The condition tree evaluated against an extracted value.</param>
/// </summary>
public sealed record PredefinedRoiValidationConditionDto(
    Guid Id,
    string Code,
    string Label,
    ERoiType RoiType,
    RoiValidationConditionNode Condition
);
