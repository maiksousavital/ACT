using ACT.Domain.Enums;

namespace ACT.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int? CompanyId { get; set; }
    public Company? Company { get; set; }
    public Role Role { get; set; } = Role.User;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Bumped on deactivate/role/company change and on logout — included as a JWT claim so
    // already-issued tokens can be rejected before their natural expiry (see JwtService,
    // Program.cs OnTokenValidated, and AuthService.LogoutAsync).
    public int TokenVersion { get; set; } = 0;
}

