// ACT.Infrastructure/Persistence/AppDbContextFactory.cs
using ACT.Application.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ACT.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    // Design-time tooling (`dotnet ef migrations ...`) has no HTTP caller to scope to —
    // treat it as an unrestricted system context, same as HttpContextTenantContext does
    // when there's no HttpContext.
    private class DesignTimeTenantContext : ITenantContext
    {
        public int? CompanyId => null;
        public bool IsSuperAdmin => true;
    }

    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=act.db")
            .Options;

        return new AppDbContext(options, new DesignTimeTenantContext());
    }
}