namespace ACT.Application.Dtos;

public class UpdateTreatmentRequest
{
    public int ClientId { get; set; }
    public int TreatmentTypeId { get; set; }
    public DateTime TreatmentDate { get; set; }
    public string? Notes { get; set; }
    public DateTime NextFollowUpDate { get; set; }
    public DateTime? FollowedUpAt { get; set; }
    public string? FollowUpNotes { get; set; }
}

