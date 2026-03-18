using System.ComponentModel.DataAnnotations.Schema;
using WebFormHTR.Domain.Entities.Base;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Domain.Entities;

public class Logsheet : BaseEntity
{
    public Guid TemplateId { get; set; }
    public virtual Template Template { get; set; }

    public Guid FileId { get; set; }
    public virtual File File { get; set; }

    public ELogSheetStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public AlignmentContainer AlignmentData { get; set; } = new(null, null);

    public string? ErrorMessage { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public virtual ICollection<ExtractedValue> ExtractedValues { get; set; } =
        new List<ExtractedValue>();

    public bool CanBeProcessed()
    {
        return Status is ELogSheetStatus.Pending or ELogSheetStatus.Failed;
    }

    public bool CanBeEdited()
    {
        return Status is ELogSheetStatus.Pending or ELogSheetStatus.Failed;
    }

    public bool CanBeReset()
    {
        return Status != ELogSheetStatus.Processing;
    }
}
