using System.ComponentModel.DataAnnotations;
using ACT.Domain.Enums;

namespace ACT.Application.Dtos;

public class UpdateUserRequest
{
    [EmailAddress, MaxLength(200)]
    public string? Email { get; set; }

    public int? CompanyId { get; set; }
    public Role? Role { get; set; }
    public bool? IsActive { get; set; }
}

