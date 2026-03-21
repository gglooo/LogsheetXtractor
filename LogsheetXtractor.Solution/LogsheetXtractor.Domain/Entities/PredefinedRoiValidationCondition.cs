using LogsheetXtractor.Domain.Entities.Base;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Domain.ValueObjects.RoiValidation;

namespace LogsheetXtractor.Domain.Entities;

public class PredefinedRoiValidationCondition : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public ERoiType RoiType { get; set; }
    public RoiValidationConditionNode Condition { get; set; } = null!;
}
