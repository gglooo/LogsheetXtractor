using WebFormHTR.Domain.Entities.Base;

namespace WebFormHTR.Domain.Entities;

public class Residual: BaseEntity
{
    public Guid TemplateId { get; set; }
    public virtual required Template Template { get; set; }
    public string Content { get; set; } = String.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}