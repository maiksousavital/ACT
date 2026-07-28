using System.ComponentModel.DataAnnotations;

namespace ACT.Application.Dtos;

public class CreateBrandSettingsRequest
{
    [MaxLength(20)]
    public string? PrimaryColor { get; set; }

    [MaxLength(20)]
    public string? SecondaryColor { get; set; }

    [MaxLength(20)]
    public string? AccentColor { get; set; }

    [MaxLength(20)]
    public string? SidebarColor { get; set; }

    [MaxLength(20)]
    public string? BackgroundColor { get; set; }

    [MaxLength(20)]
    public string? Theme { get; set; }

    [MaxLength(2000)]
    public string? LogoUrl { get; set; }
}

