using Microsoft.EntityFrameworkCore;
using WebFormHTR.Domain.Entities;

namespace WebFormHTR.Application.Interfaces;

public interface IAppDbContext
{
    public DbSet<Template> Templates { get; set; }
    public DbSet<Domain.Entities.File> Files { get; set; }
    public DbSet<Roi> Rois { get; set; }
    public DbSet<Residual> Residuals { get; set; }
    public DbSet<Logsheet> Logsheets { get; set; }
    public DbSet<ExtractedValue> ExtractedValues { get; set; }

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}