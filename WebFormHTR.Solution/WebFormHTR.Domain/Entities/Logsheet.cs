using System.ComponentModel.DataAnnotations.Schema;
using WebFormHTR.Domain.Entities.Base;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Domain.Entities;

public class Logsheet : BaseEntity
{
    public Guid TemplateId { get; set; }
    public virtual Template Template { get; set; }

    public Guid? BacksideTemplateId { get; set; }
    public virtual Template? BacksideTemplate { get; set; }

    public Guid FileId { get; set; }
    public virtual File File { get; set; }

    public ELogSheetStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? AlignmentData { get; set; }

    [NotMapped]
    public AlignmentContainer AlignmentDataModelConfig
    {
        get => AlignmentContainer.FromJson(AlignmentData);
        set => AlignmentData = value.ToJson();
    }

    public string? ErrorMessage { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public virtual ICollection<ExtractedValue> ExtractedValues { get; set; } =
        new List<ExtractedValue>();
}