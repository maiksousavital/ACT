using ACT.Domain.Entities;

namespace ACT.Domain.Interfaces;

public interface ITreatmentRepository
{
    Task<IEnumerable<Treatment>> GetDueAsync(int companyId);
    Task<IEnumerable<Treatment>> GetTodayAsync(int companyId);
    Task<IEnumerable<Treatment>> GetByClientAsync(int clientId);
    Task<Treatment?> GetByIdAsync(int id);
    Task AddAsync(Treatment treatment);
    Task UpdateAsync(Treatment treatment);
    Task SaveChangesAsync();
    Task<(IEnumerable<Treatment> Items, int TotalCount)> GetPagedAsync(int companyId, int page, int pageSize);
}