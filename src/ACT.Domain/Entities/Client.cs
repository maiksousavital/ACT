namespace ACT.Domain.Entities;


public class Client
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
    public bool IsArchived { get; set; } = false;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public ICollection<Treatment> Treatments { get; set; } = new List<Treatment>();

    public string FullName => $"{FirstName} {LastName}";
}