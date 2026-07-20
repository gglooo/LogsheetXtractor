using System.Text.Json;
using LogsheetXtractor.Domain.Enums;

namespace LogsheetXtractor.Application.Features.RoiValidation.DTOs;

/// <summary>
/// Catalog of validation rules supported by the application.
/// <param name="Version">The catalog version exposed to clients.</param>
/// <param name="RoiTypes">Validation rules grouped by supported ROI type.</param>
/// </summary>
public sealed record RoiValidationRuleCatalogDto(
    string Version,
    IReadOnlyList<RoiValidationRulesByRoiTypeDto> RoiTypes
);

/// <summary>
/// Validation rules applicable to one ROI type.
/// <param name="RoiType">The ROI type supported by the rules.</param>
/// <param name="Rules">The rule definitions available for that ROI type.</param>
/// </summary>
public sealed record RoiValidationRulesByRoiTypeDto(
    ERoiType RoiType,
    IReadOnlyList<RoiValidationRuleDefinitionDto> Rules
);

/// <summary>
/// Client-facing description of one ROI validation rule.
/// <param name="RuleType">The stable identifier used to select the rule evaluator.</param>
/// <param name="Label">The human-readable rule label.</param>
/// <param name="Description">The human-readable explanation of the rule.</param>
/// <param name="DefaultParams">The default JSON parameters for the rule.</param>
/// </summary>
public sealed record RoiValidationRuleDefinitionDto(
    string RuleType,
    string Label,
    string Description,
    JsonElement DefaultParams
);
