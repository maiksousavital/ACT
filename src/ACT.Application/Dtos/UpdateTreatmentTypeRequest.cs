using System.ComponentModel.DataAnnotations;

namespace ACT.Application.Dtos;

public class UpdateTreatmentTypeRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 3650)]
    public int FollowUpIntervalDays { get; set; }

    public bool IsActive { get; set; } = true;
}

