using WebFormHTR.Domain.Entities.Base;
using WebFormHTR.Domain.Enums;

namespace WebFormHTR.Domain.Entities;

public class ExtractedValue : BaseEntity
{
    public Guid LogsheetId { get; set; }
    public virtual Logsheet Logsheet { get; set; }

    public Guid RoiId { get; set; }
    public virtual Roi Roi { get; set; }

    public string Value { get; set; } = string.Empty;
    public string? CorrectedValue { get; set; }

    // maybe add this if possible?
    // public float Confidence { get; set; }

    public EVerificationStatus Status { get; set; } = EVerificationStatus.Unverified;
    public bool IsCorrected => !string.IsNullOrEmpty(CorrectedValue);
    public bool IsBackside => Logsheet.Template.BacksideTemplate?.Rois.Any(r => r.Id == RoiId) ?? false;
}