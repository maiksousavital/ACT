namespace ACT.Domain.Entities;

// ACT.Domain/Entities/TreatmentType.cs
public class TreatmentType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int FollowUpIntervalDays { get; set; } = 7; // Default to 1 week
    public bool IsActive { get; set; } = true;

    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public ICollection<Treatment> Treatments { get; set; } = new List<Treatment>();
}