using ACT.Domain.Entities;

namespace ACT.Domain.Interfaces;

public interface ITreatmentRepository
{
    Task<IEnumerable<Treatment>> GetDueAsync();
    Task<IEnumerable<Treatment>> GetTodayAsync();
    Task<IEnumerable<Treatment>> GetByClientAsync(Guid clientId);
    Task<Treatment?> GetByIdAsync(Guid id);
    Task AddAsync(Treatment treatment);
    Task SaveChangesAsync();
}