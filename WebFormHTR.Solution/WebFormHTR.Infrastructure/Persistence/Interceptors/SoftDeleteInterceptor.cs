using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebFormHTR.Domain.Entities.Base;

namespace WebFormHTR.Infrastructure.Persistence.Interceptors;

// https://www.milanjovanovic.tech/blog/implementing-soft-delete-with-ef-core
public class SoftDeleteInterceptor: SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        IEnumerable<EntityEntry<ISoftDeletable>> entries =
            eventData
                .Context
                .ChangeTracker
                .Entries<ISoftDeletable>()
                .Where(e => e.State == EntityState.Deleted);

        foreach (EntityEntry<ISoftDeletable> entry in entries)
        {
            entry.State = EntityState.Modified;
            entry.Entity.DeletedAt = DateTime.UtcNow;
            foreach (var navigationEntry in entry.Navigations)
            {
                if (navigationEntry.Metadata.TargetEntityType.IsOwned())
                {
                    if (navigationEntry is ReferenceEntry { TargetEntry.State: EntityState.Deleted } referenceEntry)
                    {
                        referenceEntry.TargetEntry.State = EntityState.Modified;
                    }
                }
            }
            
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}