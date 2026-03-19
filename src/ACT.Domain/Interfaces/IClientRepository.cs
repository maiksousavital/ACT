using ACT.Domain.Entities;

namespace ACT.Domain.Interfaces;

public interface IClientRepository
{
    Task<IEnumerable<Client>> GetAllAsync(bool includeArchived = false);
    Task<Client?> GetByIdAsync(Guid id);
    Task AddAsync(Client client);
    Task SaveChangesAsync();
}