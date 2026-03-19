namespace ACT.Domain.Entities;

// ACT.Domain/Entities/TreatmentType.cs
public class TreatmentType
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int FollowUpIntervalMonths { get; set; } = 3;
    public bool IsActive { get; set; } = true;

    public ICollection<Treatment> Treatments { get; set; } = new List<Treatment>();
}