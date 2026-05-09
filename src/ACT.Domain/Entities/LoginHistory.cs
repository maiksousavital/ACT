namespace ACT.Domain.Entities;

public class LoginHistory
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public int? CompanyId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool Success { get; set; }
    public string? FailureReason { get; set; }               // e.g. "Invalid password", "Account inactive"
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

