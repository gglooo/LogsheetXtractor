using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;
using File = WebFormHTR.Domain.Entities.File;

namespace WebFormHTR.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Template> Templates { get; set; }
    public DbSet<File> Files { get; set; }
    public DbSet<Roi> Rois { get; set; }
    public DbSet<Residual> Residuals { get; set; }
    public DbSet<Logsheet> Logsheets { get; set; }
    public DbSet<ExtractedValue> ExtractedValues { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Roi>()
            .HasQueryFilter(e => e.DeletedAt == null)
            .OwnsOne(r => r.Coordinates);

        modelBuilder.Entity<Roi>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
            entity.OwnsOne(r => r.Coordinates);
            entity.HasOne<Template>(r => r.Template)
                .WithMany(t => t.Rois)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(r => new { r.TemplateId, r.VariableName })
                .IsUnique()
                .HasFilter("[DeletedAt] IS NULL");
        });

        modelBuilder.Entity<Residual>()
            .HasQueryFilter(e => e.DeletedAt == null)
            .OwnsOne(r => r.Coordinates);

        modelBuilder.Entity<Template>()
            .HasQueryFilter(e => e.DeletedAt == null)
            .HasOne(t => t.Parent)
            .WithMany(t => t.Children)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Template>()
            .HasOne<File>(t => t.File)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Residual>()
            .HasOne<Template>(r => r.Template)
            .WithMany(t => t.Residuals)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Logsheet>()
            .HasQueryFilter(e => e.DeletedAt == null)
            .HasMany(l => l.ExtractedValues)
            .WithOne(ev => ev.Logsheet)
            .HasForeignKey(eV => eV.LogsheetId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Logsheet>()
            .HasOne<File>(l => l.File)
            .WithOne()
            .HasForeignKey<Logsheet>(l => l.FileId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Logsheet>()
            .HasOne<Template>(l => l.Template)
            .WithMany(t => t.Logsheets)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Logsheet>()
            .HasOne<Template>(l => l.BacksideTemplate)
            .WithMany(t => t.BacksideLogsheets)
            .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<ExtractedValue>()
            .HasQueryFilter(e => e.DeletedAt == null)
            .HasOne<Roi>(ev => ev.Roi)
            .WithMany()
            .HasForeignKey(ev => ev.RoiId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
}