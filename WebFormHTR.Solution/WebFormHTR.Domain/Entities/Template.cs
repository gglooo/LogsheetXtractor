using WebFormHTR.Domain.Entities.Base;

namespace WebFormHTR.Domain.Entities;

public class Template : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public int? Width { get; set; }
    public int? Height { get; set; }

    public Guid? ParentId { get; set; }
    public virtual Template? Parent { get; set; }
    public Guid FileId { get; set; }
    public virtual File File { get; set; }

    public virtual ICollection<Template> Children { get; set; } = new List<Template>();
    public virtual ICollection<Residual> Residuals { get; set; } = new List<Residual>();
    public virtual ICollection<Roi> Rois { get; set; } = new List<Roi>();
    public virtual ICollection<Logsheet> Logsheets { get; set; } = new List<Logsheet>();
    public virtual ICollection<Logsheet> BacksideLogsheets { get; set; } = new List<Logsheet>();
}