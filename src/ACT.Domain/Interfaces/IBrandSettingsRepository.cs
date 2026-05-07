using ACT.Domain.Entities;

namespace ACT.Domain.Interfaces;

public interface IBrandSettingsRepository
{
    Task<BrandSettings?> GetByCompanyIdAsync(int companyId);
    Task AddAsync(BrandSettings settings);
    Task UpdateAsync(BrandSettings settings);
    Task SaveChangesAsync();
}
