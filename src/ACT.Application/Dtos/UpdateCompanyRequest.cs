using System.ComponentModel.DataAnnotations;

namespace ACT.Application.Dtos;

public class UpdateCompanyRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    private string? _contactEmail;

    // See CreateClientRequest.Email for why blank is normalized to null.
    [EmailAddress, MaxLength(150)]
    public string? ContactEmail
    {
        get => _contactEmail;
        set => _contactEmail = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }
}

