using System.ComponentModel.DataAnnotations;

namespace ACT.Application.Dtos;

public class UpdateTreatmentRequest
{
    [Range(1, int.MaxValue)]
    public int ClientId { get; set; }

    [Range(1, int.MaxValue)]
    public int TreatmentTypeId { get; set; }

    public DateTime TreatmentDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

