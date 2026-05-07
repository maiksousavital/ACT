using ACT.Domain.Entities;

namespace ACT.Domain.Interfaces;

public interface IClientRepository
{
    Task<IEnumerable<Client>> GetAllAsync(int companyId, bool includeArchived = false);
    Task<Client?> GetByIdAsync(int id);
    Task AddAsync(Client client);
    Task UpdateAsync(Client client);
    Task SaveChangesAsync();
    Task<(IEnumerable<Client> Items, int TotalCount)> GetPagedAsync(int companyId, int page, int pageSize);
}