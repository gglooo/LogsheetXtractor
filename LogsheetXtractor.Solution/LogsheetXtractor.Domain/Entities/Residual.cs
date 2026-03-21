using LogsheetXtractor.Domain.Entities.Base;
using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Domain.Entities;

public class Residual : BaseEntity
{
    public Guid TemplateId { get; set; }
    public virtual Template Template { get; set; }
    public string Content { get; set; } = string.Empty;
    public Coordinates Coordinates { get; set; }
}