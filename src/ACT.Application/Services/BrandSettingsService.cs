using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using ACT.Domain.Entities;
using ACT.Domain.Interfaces;

namespace ACT.Application.Services;

public class BrandSettingsService : IBrandSettingsService
{
    private readonly IBrandSettingsRepository _repo;

    public BrandSettingsService(IBrandSettingsRepository repo)
    {
        _repo = repo;
    }

    public async Task<BrandSettingsDto?> GetByCompanyIdAsync(int companyId)
    {
        var settings = await _repo.GetByCompanyIdAsync(companyId);
        return settings == null ? null : ToDto(settings);
    }

    public async Task<BrandSettingsDto> CreateAsync(int companyId, CreateBrandSettingsRequest request)
    {
        var entity = new BrandSettings
        {
            CompanyId = companyId,
            PrimaryColor = request.PrimaryColor,
            SecondaryColor = request.SecondaryColor,
            AccentColor = request.AccentColor,
            Theme = request.Theme,
            LogoUrl = request.LogoUrl
        };

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<BrandSettingsDto?> UpdateAsync(int companyId, UpdateBrandSettingsRequest request)
    {
        var settings = await _repo.GetByCompanyIdAsync(companyId);
        if (settings == null) return null;

        settings.PrimaryColor = request.PrimaryColor;
        settings.SecondaryColor = request.SecondaryColor;
        settings.AccentColor = request.AccentColor;
        settings.Theme = request.Theme;
        settings.LogoUrl = request.LogoUrl;

        await _repo.UpdateAsync(settings);
        await _repo.SaveChangesAsync();
        return ToDto(settings);
    }

    private static BrandSettingsDto ToDto(BrandSettings s)
    {
        return new BrandSettingsDto
        {
            Id = s.Id,
            CompanyId = s.CompanyId,
            PrimaryColor = s.PrimaryColor,
            SecondaryColor = s.SecondaryColor,
            AccentColor = s.AccentColor,
            Theme = s.Theme,
            LogoUrl = s.LogoUrl
        };
    }
}
