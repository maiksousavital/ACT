using ACT.Domain.Enums;

namespace ACT.Application.Dtos;

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public Role Role { get; set; } = Role.Admin;
}

