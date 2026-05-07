using ACT.Domain.Entities;

namespace ACT.Application.Dtos;
public class TreatmentDto
{
    public int      Id                { get; init; }
    public int      ClientId          { get; init; }
    public string    ClientName        { get; init; } = string.Empty;
    public int      TreatmentTypeId   { get; init; }
    public string    TreatmentTypeName { get; init; } = string.Empty;
    public string?   Phone             { get; init; }
    public DateTime  TreatmentDate     { get; init; }
    public DateTime  NextFollowUpDate  { get; init; }
    public string?   Notes             { get; init; }
    public DateTime? FollowedUpAt      { get; init; }
    public string?   FollowUpNotes     { get; init; }
    public bool      IsDue             { get; init; }
    public bool      IsFollowedUp      { get; init; }

    public static TreatmentDto FromEntity(Treatment t) => new TreatmentDto
    {
        Id = t.Id,
        ClientId = t.ClientId,
        TreatmentTypeId = t.TreatmentTypeId,
        TreatmentDate = t.TreatmentDate,
        NextFollowUpDate = t.NextFollowUpDate,
        Notes = t.Notes,
        FollowedUpAt = t.FollowedUpAt,
        FollowUpNotes = t.FollowUpNotes,
        IsFollowedUp = t.IsFollowedUp,
        IsDue = t.IsDue
    };
}

public class CreateTreatmentRequest
{
    public int ClientId { get; set; }
    public int TreatmentTypeId { get; set; }
    public DateTime TreatmentDate { get; set; }
    public string? Notes { get; set; }
}


public record CompleteFollowUpRequest(
    string? FollowUpNotes
);