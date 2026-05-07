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
    public DbSet<BrandSettings> BrandSettings => Set<BrandSettings>();
    public DbSet<Company> Companies => Set<Company>();

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
             .OnDelete(DeleteBehavior.Cascade)
             .IsRequired(false); // Make optional

            e.HasOne(c => c.Company)
             .WithMany(co => co.Clients)
             .HasForeignKey(c => c.CompanyId)
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

            e.HasOne(t => t.Company)
             .WithMany(co => co.TreatmentTypes)
             .HasForeignKey(t => t.CompanyId)
             .OnDelete(DeleteBehavior.Cascade);
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

            e.HasOne(t => t.Company)
             .WithMany(co => co.Treatments)
             .HasForeignKey(t => t.CompanyId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Company ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Company>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(200);
            e.Property(c => c.ContactEmail).HasMaxLength(150);
            e.Property(c => c.Phone).HasMaxLength(50);
            e.Property(c => c.Address).HasMaxLength(300);
        });

        // ── BrandSettings ─────────────────────────────────────────────────────
        modelBuilder.Entity<BrandSettings>(e =>
        {
            e.HasKey(b => b.Id);
            e.HasOne(b => b.Company)
             .WithMany(c => c.BrandSettings)
             .HasForeignKey(b => b.CompanyId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Seed data ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Company>().HasData(
            new Company
            {
                Id = 1,
                Name = "Default Company"
            }
        );

        modelBuilder.Entity<TreatmentType>().HasData(
            new TreatmentType
            {
                Id = 1,
                Name = "Botox",
                FollowUpIntervalDays = 90,
                IsActive = true,
                CompanyId = 1
            },
            new TreatmentType
            {
                Id = 2,
                Name = "Filler",
                FollowUpIntervalDays = 180,
                IsActive = true,
                CompanyId = 1
            },
            new TreatmentType
            {
                Id = 3,
                Name = "Skin Booster",
                FollowUpIntervalDays = 120,
                IsActive = true,
                CompanyId = 1
            },
            new TreatmentType
            {
                Id = 4,
                Name = "Peel",
                FollowUpIntervalDays = 30,
                IsActive = true,
                CompanyId = 1
            }
        );
    }
}