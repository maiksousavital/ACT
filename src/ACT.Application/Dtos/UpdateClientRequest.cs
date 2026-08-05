using System.ComponentModel.DataAnnotations;

namespace ACT.Application.Dtos;

public class UpdateClientRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    private string? _email;

    // See CreateClientRequest.Email for why blank is normalized to null.
    [EmailAddress, MaxLength(150)]
    public string? Email
    {
        get => _email;
        set => _email = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

