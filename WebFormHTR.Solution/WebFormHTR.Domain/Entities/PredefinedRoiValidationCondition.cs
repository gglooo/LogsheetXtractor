using WebFormHTR.Domain.Entities.Base;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Domain.ValueObjects.RoiValidation;

namespace WebFormHTR.Domain.Entities;

public class PredefinedRoiValidationCondition : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public ERoiType RoiType { get; set; }
    public RoiValidationConditionNode Condition { get; set; } = null!;
}
