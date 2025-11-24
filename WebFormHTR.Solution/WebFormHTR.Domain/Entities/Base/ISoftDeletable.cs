namespace WebFormHTR.Domain.Entities.Base;

public interface ISoftDeletable
{
    public DateTime? DeletedAt { get; set; }

    bool IsDeleted() => DeletedAt.HasValue;
}