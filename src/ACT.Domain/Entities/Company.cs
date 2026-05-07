namespace ACT.Domain.Entities;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public ICollection<BrandSettings> BrandSettings { get; set; } = new List<BrandSettings>();
    public ICollection<Client> Clients { get; set; } = new List<Client>();
    public ICollection<TreatmentType> TreatmentTypes { get; set; } = new List<TreatmentType>();
    public ICollection<Treatment> Treatments { get; set; } = new List<Treatment>();
}

