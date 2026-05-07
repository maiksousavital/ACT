using ACT.Domain.Entities;
using ACT.Domain.Interfaces;
using ACT.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ACT.Infrastructure.Repositories;

public class BrandSettingsRepository : IBrandSettingsRepository
{
    private readonly AppDbContext _db;
    public BrandSettingsRepository(AppDbContext db) => _db = db;

    public async Task<BrandSettings?> GetByCompanyIdAsync(int companyId)
        => await _db.BrandSettings.FirstOrDefaultAsync(x => x.CompanyId == companyId);

    public async Task AddAsync(BrandSettings settings)
        => await _db.BrandSettings.AddAsync(settings);

    public async Task UpdateAsync(BrandSettings settings)
    {
        _db.BrandSettings.Update(settings);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
}
