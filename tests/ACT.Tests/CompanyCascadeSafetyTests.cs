using ACT.Application.Services;
using ACT.Application.Services.Interfaces;
using ACT.Domain.Entities;
using ACT.Infrastructure.Persistence;
using ACT.Infrastructure.Repositories;
using ACT.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace ACT.Tests;

/// <summary>
/// B7 — Company -> Client/Treatment/TreatmentType/BrandSettings FKs are all configured with
/// OnDelete(Cascade) at the DB level (see AppDbContext), while CompanyService.DeleteAsync only
/// ever soft-deletes (IsDeleted = true). That's consistent today, but nothing stops a future
/// `DbContext.Remove(company)` or raw SQL delete from cascading a full, unrecoverable wipe of a
/// tenant's data. This test guards CompanyService's contract: delete must never remove rows.
/// </summary>
public class CompanyCascadeSafetyTests
{
    private class SystemTenantContext : ITenantContext
    {
        public int? CompanyId => null;
        public bool IsSuperAdmin => true;
    }

    private static AppDbContext BuildContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options, new SystemTenantContext());
    }

    [Fact]
    public async Task CompanyService_DeleteAsync_SoftDeletesOnly_NeverCascadesToChildren()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var seed = BuildContext(dbName))
        {
            seed.Companies.Add(new Company { Id = 1, Name = "Acme" });
            seed.Clients.Add(new Client { Id = 1, FirstName = "A", LastName = "B", CompanyId = 1 });
            seed.TreatmentTypes.Add(new TreatmentType { Id = 1, Name = "Botox", FollowUpIntervalDays = 90, IsActive = true, CompanyId = 1 });
            await seed.SaveChangesAsync();
        }

        using (var context = BuildContext(dbName))
        {
            var service = new CompanyService(new CompanyRepository(context), new NoopAuditService());
            var deleted = await service.DeleteAsync(1, null, "test@test.com");
            Assert.True(deleted);
        }

        using var verify = BuildContext(dbName);
        var company = await verify.Companies.IgnoreQueryFilters().AsNoTracking().FirstAsync(c => c.Id == 1);
        Assert.True(company.IsDeleted); // soft-deleted — row still present

        // A hard delete/cascade would have wiped these; they must still be here.
        Assert.Equal(1, await verify.Clients.IgnoreQueryFilters().CountAsync(c => c.Id == 1));
        Assert.Equal(1, await verify.TreatmentTypes.IgnoreQueryFilters().CountAsync(t => t.Id == 1));
    }
}
