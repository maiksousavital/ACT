using ACT.Domain.Entities;

namespace ACT.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
    Task<IEnumerable<AuditLog>> GetByCompanyAsync(int companyId, int limit = 100);
    Task<IEnumerable<AuditLog>> GetAllAsync(int limit = 100);
    Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedAsync(int? companyId, int page, int pageSize);
    Task SaveChangesAsync();
}

