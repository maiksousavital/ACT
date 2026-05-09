namespace ACT.Application.Dtos;

public class BrandSettingsDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? Theme { get; set; }
    public string? LogoUrl { get; set; }
}

