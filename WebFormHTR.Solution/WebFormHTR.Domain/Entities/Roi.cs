using WebFormHTR.Domain.Entities.Base;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Domain.Entities;

public class Roi : BaseEntity
{
    public Guid TemplateId { get; set; }
    public virtual Template Template { get; set; }
    public string VariableName { get; set; } = string.Empty;
    public ERoiType Type { get; set; }

    public Coordinates Coordinates { get; set; }

}