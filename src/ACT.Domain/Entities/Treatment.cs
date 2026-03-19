namespace ACT.Domain.Entities;


public class Treatment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public Guid TreatmentTypeId { get; set; }
    public TreatmentType TreatmentType { get; set; } = null!;

    public DateTime TreatmentDate { get; set; }
    public DateTime NextFollowUpDate { get; set; }
    public string? Notes { get; set; }

    // Follow-up outcome — no separate table needed
    public DateTime? FollowedUpAt { get; set; }
    public string? FollowUpNotes { get; set; }

    public bool IsFollowedUp => FollowedUpAt.HasValue;
    public bool IsDue => !IsFollowedUp && NextFollowUpDate.Date <= DateTime.UtcNow.Date;

    // Factory method — interval comes from the type, not hardcoded
    public static Treatment Create(
        Guid clientId,
        TreatmentType type,
        DateTime date,
        string? notes = null)
    {
        return new Treatment
        {
            ClientId = clientId,
            TreatmentTypeId = type.Id,
            TreatmentType = type,
            TreatmentDate = date,
            NextFollowUpDate = date.AddMonths(type.FollowUpIntervalMonths),
            Notes = notes
        };
    }
}