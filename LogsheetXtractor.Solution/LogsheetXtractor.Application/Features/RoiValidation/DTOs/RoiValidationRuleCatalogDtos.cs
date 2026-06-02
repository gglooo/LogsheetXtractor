using System.Text.Json;
using LogsheetXtractor.Domain.Enums;

namespace LogsheetXtractor.Application.Features.RoiValidation.DTOs;

/// <summary>
/// TODO-DOC: Describe RoiValidationRuleCatalogDto purpose and usage.
/// <param name="Version">TODO-DOC: Describe Version.</param>
/// <param name="RoiTypes">TODO-DOC: Describe RoiTypes.</param>
/// </summary>
public sealed record RoiValidationRuleCatalogDto(
    string Version,
    IReadOnlyList<RoiValidationRulesByRoiTypeDto> RoiTypes
);

/// <summary>
/// TODO-DOC: Describe RoiValidationRulesByRoiTypeDto purpose and usage.
/// <param name="RoiType">TODO-DOC: Describe RoiType.</param>
/// <param name="Rules">TODO-DOC: Describe Rules.</param>
/// </summary>
public sealed record RoiValidationRulesByRoiTypeDto(
    ERoiType RoiType,
    IReadOnlyList<RoiValidationRuleDefinitionDto> Rules
);

/// <summary>
/// TODO-DOC: Describe RoiValidationRuleDefinitionDto purpose and usage.
/// <param name="RuleType">TODO-DOC: Describe RuleType.</param>
/// <param name="Label">TODO-DOC: Describe Label.</param>
/// <param name="Description">TODO-DOC: Describe Description.</param>
/// <param name="DefaultParams">TODO-DOC: Describe DefaultParams.</param>
/// </summary>
public sealed record RoiValidationRuleDefinitionDto(
    string RuleType,
    string Label,
    string Description,
    JsonElement DefaultParams
);
