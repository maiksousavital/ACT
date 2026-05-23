namespace ACT.Domain.Entities;

public class BrandSettings
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? SidebarColor { get; set; }
    public string? BackgroundColor { get; set; }
    public string? Theme { get; set; } // e.g., "light", "dark", "custom"
    public string? LogoUrl { get; set; }
}
