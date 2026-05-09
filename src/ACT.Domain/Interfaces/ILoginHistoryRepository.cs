using ACT.Domain.Entities;

namespace ACT.Domain.Interfaces;

public interface ILoginHistoryRepository
{
    Task AddAsync(LoginHistory record);
    Task<IEnumerable<LoginHistory>> GetByUserAsync(int userId, int limit = 50);
    Task<IEnumerable<LoginHistory>> GetByCompanyAsync(int companyId, int limit = 100);
    Task<IEnumerable<LoginHistory>> GetAllAsync(int limit = 100);
    Task<(IEnumerable<LoginHistory> Items, int TotalCount)> GetPagedAsync(int? companyId, int page, int pageSize);
    Task SaveChangesAsync();
}

