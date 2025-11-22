using WebFormHTR.Domain.Entities.Base;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Domain.Entities;

public class Roi: BaseEntity
{
    public Guid TemplateId { get; set; }
    public virtual required Template Template { get; set; }
    public string VariableName { get; set; } = String.Empty;
    public ERoiType Type { get; set; }
    
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    
    public Guid? ExtractedValueId { get; set; }
    public virtual ExtractedValue? ExtractedValue { get; set; }
}