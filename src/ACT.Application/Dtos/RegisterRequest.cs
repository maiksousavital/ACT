using System.ComponentModel.DataAnnotations;
using ACT.Domain.Enums;

namespace ACT.Application.Dtos;

public class RegisterRequest
{
    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(200)]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$",
        ErrorMessage = "Password must include at least one letter and one number.")]
    public string Password { get; set; } = string.Empty;

    public int CompanyId { get; set; }
    public Role Role { get; set; } = Role.Admin;
}

