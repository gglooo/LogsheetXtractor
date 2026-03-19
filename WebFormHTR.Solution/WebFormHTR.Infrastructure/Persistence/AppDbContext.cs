using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.ValueObjects;
using WebFormHTR.Domain.ValueObjects.RoiValidation;
using File = WebFormHTR.Domain.Entities.File;

namespace WebFormHTR.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private static readonly ValueComparer<RoiValidationConditionNode?> RoiValidationConditionComparer =
        new(
            (left, right) => SerializeValidationCondition(left) == SerializeValidationCondition(right),
            value => ComputeValidationConditionHash(value),
            value => DeserializeValidationCondition(SerializeValidationCondition(value))
        );

    public DbSet<Template> Templates { get; set; }
    public DbSet<File> Files { get; set; }
    public DbSet<Roi> Rois { get; set; }
    public DbSet<Residual> Residuals { get; set; }
    public DbSet<Logsheet> Logsheets { get; set; }
    public DbSet<ExtractedValue> ExtractedValues { get; set; }
    public DbSet<PredefinedRoiValidationCondition> PredefinedRoiValidationConditions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Roi>()
            .HasQueryFilter(e => e.DeletedAt == null)
            .OwnsOne(r => r.Coordinates);

        modelBuilder.Entity<Roi>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
            entity.OwnsOne(r => r.Coordinates);
            entity.Property(r => r.ValidationCondition)
                .HasConversion(
                    v => SerializeValidationCondition(v),
                    v => DeserializeValidationCondition(v));
            entity.Property(r => r.ValidationCondition)
                .Metadata.SetValueComparer(RoiValidationConditionComparer);
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
            .HasForeignKey(t => t.ParentId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Template>()
            .HasQueryFilter(e => e.DeletedAt == null)
            .HasOne<Template>(t => t.BacksideTemplate)
            .WithOne(t => t.FrontsideTemplate)
            .HasForeignKey<Template>(t => t.BacksideTemplateId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Template>()
            .HasOne<File>(t => t.File)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Template>()
            .HasIndex(t => t.Name)
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");

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
            .HasForeignKey(l => l.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Logsheet>()
            .Property(l => l.AlignmentData)
            .HasConversion(
                v => JsonSerializer.Serialize(v,
                    new JsonSerializerOptions
                        { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = false }),
                v => string.IsNullOrWhiteSpace(v)
                    ? new AlignmentContainer(null, null)
                    : JsonSerializer.Deserialize<AlignmentContainer>(v,
                          new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                      ?? new AlignmentContainer(null, null)
            );

        modelBuilder.Entity<ExtractedValue>()
            .HasQueryFilter(e => e.DeletedAt == null)
            .Property(e => e.ValidationWarnings)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => string.IsNullOrWhiteSpace(v)
                    ? new List<RoiValidationWarningSnapshot>()
                    : JsonSerializer.Deserialize<List<RoiValidationWarningSnapshot>>(v, JsonOptions) ??
                      new List<RoiValidationWarningSnapshot>());

        modelBuilder.Entity<ExtractedValue>()
            .Property(e => e.ValidationRulesVersion);

        modelBuilder.Entity<ExtractedValue>()
            .HasQueryFilter(e => e.DeletedAt == null)
            .HasOne<Roi>(ev => ev.Roi)
            .WithMany()
            .HasForeignKey(ev => ev.RoiId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PredefinedRoiValidationCondition>(entity =>
        {
            entity.Property(p => p.Code)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(p => p.Label)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(p => p.Condition)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    v => JsonSerializer.Deserialize<RoiValidationConditionNode>(v, JsonOptions)!);

            entity.HasIndex(p => new { p.Code, p.RoiType })
                .IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }

    private static string? SerializeValidationCondition(RoiValidationConditionNode? value)
    {
        return value == null ? null : JsonSerializer.Serialize(value, JsonOptions);
    }

    private static RoiValidationConditionNode? DeserializeValidationCondition(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : JsonSerializer.Deserialize<RoiValidationConditionNode>(value, JsonOptions);
    }

    private static int ComputeValidationConditionHash(RoiValidationConditionNode? value)
    {
        var serialized = SerializeValidationCondition(value);
        return serialized == null ? 0 : serialized.GetHashCode();
    }
}
