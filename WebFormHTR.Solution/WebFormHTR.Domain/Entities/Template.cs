using WebFormHTR.Domain.Entities.Base;

namespace WebFormHTR.Domain.Entities;

public class Template : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public int? Width { get; set; }
    public int? Height { get; set; }

    public Guid? ParentId { get; set; }
    public virtual Template? Parent { get; set; }
    public Guid? BacksideTemplateId { get; private set; }
    public virtual Template? BacksideTemplate { get; private set; }
    public virtual Template? FrontsideTemplate { get; set; }
    public Guid FileId { get; set; }
    public virtual File File { get; set; }

    public virtual ICollection<Template> Children { get; set; } = new List<Template>();
    public virtual ICollection<Residual> Residuals { get; set; } = new List<Residual>();
    public virtual ICollection<Roi> Rois { get; set; } = new List<Roi>();
    public virtual ICollection<Logsheet> Logsheets { get; set; } = new List<Logsheet>();

    public void SetBacksideTemplate(Template? backsideTemplate)
    {
        if (backsideTemplate == null)
        {
            BacksideTemplate = null;
            BacksideTemplateId = null;
            return;
        }

        if (backsideTemplate.Id == Id)
        {
            throw new InvalidOperationException("A template cannot be its own backside.");
        }

        if (backsideTemplate.BacksideTemplateId.HasValue || backsideTemplate.BacksideTemplate != null)
        {
            throw new InvalidOperationException(
                $"Template '{backsideTemplate.Name}' already has a backside and cannot be used as a backside.");
        }

        BacksideTemplate = backsideTemplate;
        BacksideTemplateId = backsideTemplate.Id;
    }

    public void ForceSetBacksideTemplate(Template? backsideTemplate)
    {
        if (backsideTemplate == null)
        {
            BacksideTemplate = null;
            BacksideTemplateId = null;
            return;
        }

        BacksideTemplate = backsideTemplate;
        BacksideTemplateId = backsideTemplate.Id;
    }
}