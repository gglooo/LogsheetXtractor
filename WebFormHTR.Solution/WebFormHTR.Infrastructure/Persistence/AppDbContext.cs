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
            .OwnsOne(r => r.Coordinates);
        
        modelBuilder.Entity<Residual>()
            .OwnsOne(r => r.Coordinates);
        
        modelBuilder.Entity<Template>()
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
        
        modelBuilder.Entity<Roi>()
            .HasOne<Template>(r => r.Template)
            .WithMany(t => t.Rois)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Logsheet>()
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
        
        modelBuilder.Entity<ExtractedValue>()
            .HasOne<Roi>(ev => ev.Roi)
            .WithOne(r => r.ExtractedValue)
            .HasForeignKey<Roi>(r => r.ExtractedValueId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
}