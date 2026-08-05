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
        // Upsert: a company can only ever have one BrandSettings row (enforced by convention, not
        // a DB constraint), so treat a second Create as an Update rather than risk a duplicate row
        // that GetByCompanyIdAsync's FirstOrDefaultAsync would then silently ignore.
        var existing = await _repo.GetByCompanyIdAsync(companyId);
        if (existing != null)
        {
            existing.PrimaryColor = request.PrimaryColor;
            existing.SecondaryColor = request.SecondaryColor;
            existing.AccentColor = request.AccentColor;
            existing.SidebarColor = request.SidebarColor;
            existing.BackgroundColor = request.BackgroundColor;
            existing.Theme = request.Theme;
            existing.LogoUrl = request.LogoUrl;
            await _repo.UpdateAsync(existing);
            await _repo.SaveChangesAsync();
            return ToDto(existing);
        }

        var entity = new BrandSettings
        {
            CompanyId = companyId,
            PrimaryColor = request.PrimaryColor,
            SecondaryColor = request.SecondaryColor,
            AccentColor = request.AccentColor,
            SidebarColor = request.SidebarColor,
            BackgroundColor = request.BackgroundColor,
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
        settings.SidebarColor = request.SidebarColor;
        settings.BackgroundColor = request.BackgroundColor;
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
            SidebarColor = s.SidebarColor,
            BackgroundColor = s.BackgroundColor,
            Theme = s.Theme,
            LogoUrl = s.LogoUrl
        };
    }
}
