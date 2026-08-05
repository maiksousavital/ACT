using System.ComponentModel.DataAnnotations;
using ACT.Domain.Enums;

namespace ACT.Application.Dtos;

public class UpdateUserRequest
{
    private string? _email;

    // See CreateClientRequest.Email for why blank is normalized to null.
    [EmailAddress, MaxLength(200)]
    public string? Email
    {
        get => _email;
        set => _email = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public int? CompanyId { get; set; }
    public Role? Role { get; set; }
    public bool? IsActive { get; set; }
}

