namespace ACT.Application.Dtos;

public class UpdateBrandSettingsRequest
{
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? Theme { get; set; }
    public string? LogoUrl { get; set; }
}

