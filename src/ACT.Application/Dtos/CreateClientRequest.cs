using System.ComponentModel.DataAnnotations;

namespace ACT.Application.Dtos;

public class CreateClientRequest
{
    // Only honored when the caller is SuperAdmin (who has no company of their own) — see
    // ClientController.Create. Ignored for everyone else, whose companyId always comes from
    // their JWT claim, so a regular user can't reassign which company a client belongs to.
    public int? CompanyId { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    private string? _email;

    // Blank/whitespace is normalized to null here so an optional email field left empty in the
    // UI (which submits "" rather than omitting the key) doesn't fail [EmailAddress] validation,
    // which only tolerates null, not empty string.
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

