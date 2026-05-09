using ACT.Application.Dtos;

namespace ACT.Application.Services.Interfaces;

public interface IBrandSettingsService
{
    Task<BrandSettingsDto?> GetByCompanyIdAsync(int companyId);
    Task<BrandSettingsDto> CreateAsync(int companyId, CreateBrandSettingsRequest request);
    Task<BrandSettingsDto?> UpdateAsync(int companyId, UpdateBrandSettingsRequest request);
}
