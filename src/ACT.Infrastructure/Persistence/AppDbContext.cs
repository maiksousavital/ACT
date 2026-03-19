// ACT.Infrastructure/Persistence/AppDbContext.cs
using ACT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ACT.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<TreatmentType> TreatmentTypes => Set<TreatmentType>();
    public DbSet<Treatment> Treatments => Set<Treatment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Client ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Client>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
            e.Property(c => c.LastName).IsRequired().HasMaxLength(100);
            e.Property(c => c.Phone).HasMaxLength(20);
            e.Property(c => c.Email).HasMaxLength(150);
            e.Property(c => c.Notes).HasMaxLength(1000);

            // Global query filter — archived clients are invisible
            // unless explicitly queried with .IgnoreQueryFilters()
            e.HasQueryFilter(c => !c.IsArchived);

            e.HasMany(c => c.Treatments)
             .WithOne(t => t.Client)
             .HasForeignKey(t => t.ClientId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── TreatmentType ─────────────────────────────────────────────────────
        modelBuilder.Entity<TreatmentType>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Name).IsRequired().HasMaxLength(100);

            // Prevent duplicate treatment type names
            e.HasIndex(t => t.Name).IsUnique();

            e.HasMany(t => t.Treatments)
             .WithOne(t => t.TreatmentType)
             .HasForeignKey(t => t.TreatmentTypeId)
             // Restrict prevents accidental deletion of a type that has treatments
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Treatment ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Treatment>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Notes).HasMaxLength(1000);
            e.Property(t => t.FollowUpNotes).HasMaxLength(1000);

            // Index for the most common query — due follow-ups
            e.HasIndex(t => t.NextFollowUpDate);
            e.HasIndex(t => t.ClientId);
        });

        // ── Seed data ─────────────────────────────────────────────────────────
        // Fixed GUIDs are required — EF seed data must be stable across migrations
        modelBuilder.Entity<TreatmentType>().HasData(
            new TreatmentType
            {
                Id = Guid.Parse("11111111-0000-0000-0000-000000000001"),
                Name = "Botox",
                FollowUpIntervalMonths = 3,
                IsActive = true
            },
            new TreatmentType
            {
                Id = Guid.Parse("11111111-0000-0000-0000-000000000002"),
                Name = "Filler",
                FollowUpIntervalMonths = 6,
                IsActive = true
            },
            new TreatmentType
            {
                Id = Guid.Parse("11111111-0000-0000-0000-000000000003"),
                Name = "Skin Booster",
                FollowUpIntervalMonths = 4,
                IsActive = true
            },
            new TreatmentType
            {
                Id = Guid.Parse("11111111-0000-0000-0000-000000000004"),
                Name = "Peel",
                FollowUpIntervalMonths = 1,
                IsActive = true
            }
        );
    }
}