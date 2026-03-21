using LogsheetXtractor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LogsheetXtractor.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Template> Templates { get; set; }
    DbSet<LogsheetXtractor.Domain.Entities.File> Files { get; set; }
    DbSet<Roi> Rois { get; set; }
    DbSet<Residual> Residuals { get; set; }
    DbSet<Logsheet> Logsheets { get; set; }
    DbSet<ExtractedValue> ExtractedValues { get; set; }
    DbSet<PredefinedRoiValidationCondition> PredefinedRoiValidationConditions { get; set; }

    ChangeTracker ChangeTracker { get; }

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
