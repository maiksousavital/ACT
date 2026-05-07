using ACT.Domain.Entities;

namespace ACT.Application.Services.Interfaces;

public interface IBrandSettingsService
{
    Task<BrandSettings?> GetByCompanyIdAsync(int companyId);
    Task<BrandSettings> CreateAsync(BrandSettings settings);
    Task<BrandSettings?> UpdateAsync(int companyId, BrandSettings updated);
}
