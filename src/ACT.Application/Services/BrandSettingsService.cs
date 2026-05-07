using ACT.Application.Services.Interfaces;
using ACT.Domain.Entities;
using ACT.Domain.Interfaces;

namespace ACT.Application.Services;

public class BrandSettingsService : IBrandSettingsService
{
    private readonly IBrandSettingsRepository _repo;
    public BrandSettingsService(IBrandSettingsRepository repo) => _repo = repo;

    public async Task<BrandSettings?> GetByCompanyIdAsync(int companyId)
        => await _repo.GetByCompanyIdAsync(companyId);

    public async Task<BrandSettings> CreateAsync(BrandSettings settings)
    {
        await _repo.AddAsync(settings);
        await _repo.SaveChangesAsync();
        return settings;
    }

    public async Task<BrandSettings?> UpdateAsync(int companyId, BrandSettings updated)
    {
        var settings = await _repo.GetByCompanyIdAsync(companyId);
        if (settings == null) return null;
        settings.PrimaryColor = updated.PrimaryColor;
        settings.SecondaryColor = updated.SecondaryColor;
        settings.AccentColor = updated.AccentColor;
        settings.Theme = updated.Theme;
        settings.LogoUrl = updated.LogoUrl;
        await _repo.UpdateAsync(settings);
        await _repo.SaveChangesAsync();
        return settings;
    }
}
