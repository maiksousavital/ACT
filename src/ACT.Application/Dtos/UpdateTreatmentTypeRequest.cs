namespace ACT.Application.Dtos;

public class UpdateTreatmentTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public int FollowUpIntervalDays { get; set; }
    public bool IsActive { get; set; } = true;
}

