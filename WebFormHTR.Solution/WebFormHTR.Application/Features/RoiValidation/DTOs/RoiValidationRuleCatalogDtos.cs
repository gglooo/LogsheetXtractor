using System.Text.Json;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Application.Features.RoiValidation.DTOs;

public sealed record RoiValidationRuleCatalogDto(
    string Version,
    IReadOnlyList<RoiValidationRulesByRoiTypeDto> RoiTypes
);

public sealed record RoiValidationRulesByRoiTypeDto(
    ERoiType RoiType,
    IReadOnlyList<RoiValidationRuleDefinitionDto> Rules
);

public sealed record RoiValidationRuleDefinitionDto(
    string RuleType,
    string Label,
    string Description,
    JsonElement DefaultParams
);
