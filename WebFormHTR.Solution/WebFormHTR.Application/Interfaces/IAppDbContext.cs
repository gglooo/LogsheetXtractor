using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WebFormHTR.Domain.Entities;

namespace WebFormHTR.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Template> Templates { get; set; }
    DbSet<Domain.Entities.File> Files { get; set; }
    DbSet<Roi> Rois { get; set; }
    DbSet<Residual> Residuals { get; set; }
    DbSet<Logsheet> Logsheets { get; set; }
    DbSet<ExtractedValue> ExtractedValues { get; set; }

    ChangeTracker ChangeTracker { get; }

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}