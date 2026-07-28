using System.ComponentModel.DataAnnotations;

namespace ACT.Application.Dtos;

public class UpdateClientRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress, MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

