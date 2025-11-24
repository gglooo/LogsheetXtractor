using WebFormHTR.Domain.Entities.Base;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Domain.Entities;

public class Residual: BaseEntity
{
    public Guid TemplateId { get; set; }
    public virtual required Template Template { get; set; }
    public string Content { get; set; } = String.Empty;
    public Coordinates Coordinates { get; set; }
}