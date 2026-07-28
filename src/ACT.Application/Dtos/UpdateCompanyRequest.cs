using System.ComponentModel.DataAnnotations;

namespace ACT.Application.Dtos;

public class UpdateCompanyRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress, MaxLength(150)]
    public string? ContactEmail { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }
}

