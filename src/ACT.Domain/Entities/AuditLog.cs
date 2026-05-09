namespace ACT.Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public int? CompanyId { get; set; }
    public string Action { get; set; } = string.Empty;       // e.g. "Create", "Update", "Delete", "Deactivate"
    public string EntityType { get; set; } = string.Empty;    // e.g. "Client", "Treatment", "User"
    public int? EntityId { get; set; }                        // ID of the affected record
    public string? Details { get; set; }                      // Optional extra info / JSON payload
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

