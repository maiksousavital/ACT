namespace ACT.Application.Dtos;

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public int? CompanyId { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

