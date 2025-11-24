using WebFormHTR.Domain.Entities.Base;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Domain.Entities;

public class Roi : BaseEntity
{
    public Guid TemplateId { get; set; }
    public virtual required Template Template { get; set; }
    public string VariableName { get; set; } = String.Empty;
    public ERoiType Type { get; set; }

    public Coordinates Coordinates { get; set; }

    public Guid? ExtractedValueId { get; set; }
    public virtual ExtractedValue? ExtractedValue { get; set; }
}