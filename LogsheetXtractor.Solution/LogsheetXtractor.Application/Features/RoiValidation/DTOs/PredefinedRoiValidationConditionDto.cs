using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Domain.ValueObjects.RoiValidation;

namespace LogsheetXtractor.Application.Features.RoiValidation.DTOs;

/// <summary>
/// TODO-DOC: Describe PredefinedRoiValidationConditionDto purpose and usage.
/// <param name="Id">TODO-DOC: Describe Id.</param>
/// <param name="Code">TODO-DOC: Describe Code.</param>
/// <param name="Label">TODO-DOC: Describe Label.</param>
/// <param name="RoiType">TODO-DOC: Describe RoiType.</param>
/// <param name="Condition">TODO-DOC: Describe Condition.</param>
/// </summary>
public sealed record PredefinedRoiValidationConditionDto(
    Guid Id,
    string Code,
    string Label,
    ERoiType RoiType,
    RoiValidationConditionNode Condition
);
