namespace ACT.Application.Dtos;

public class TreatmentTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int FollowUpIntervalDays { get; set; }
    public string? Notes { get; set; }

    public static IEnumerable<object> AllowedFollowUpPeriods => new[]
    {
        new { Name = "1 week", Days = 7 },
        new { Name = "2 weeks", Days = 14 },
        new { Name = "3 weeks", Days = 21 },
        new { Name = "4 weeks", Days = 28 },
        new { Name = "3 months", Days = 90 },
        new { Name = "6 months", Days = 180 },
        new { Name = "1 year", Days = 365 }
    };
}
