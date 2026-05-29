using LogsheetXtractor.Domain.Entities.Base;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Domain.ValueObjects;
using LogsheetXtractor.Domain.ValueObjects.RoiValidation;

namespace LogsheetXtractor.Domain.Entities;

public class Roi : BaseEntity
{
    public Guid TemplateId { get; set; }
    public virtual Template Template { get; set; }
    public string VariableName { get; set; } = string.Empty;
    public ERoiType Type { get; set; }

    public Coordinates Coordinates { get; set; }
    public RoiValidationConditionNode? ValidationCondition { get; set; }

}
