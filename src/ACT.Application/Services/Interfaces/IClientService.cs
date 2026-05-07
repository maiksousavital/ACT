using ACT.Application.Dtos;
using ACT.Domain.Entities;

namespace ACT.Application.Services.Interfaces;

public interface IClientService
{
    Task<IEnumerable<ClientDto>> GetAllAsync(int companyId, bool includeArchived = false);
    Task<ClientDto?> GetByIdAsync(int id);
    Task<ClientDto> CreateAsync(int companyId, Client client);
    Task UpdateAsync(int id, Client updatedClient);
    Task<PagedResult<ClientDto>> GetPagedAsync(int companyId, int page, int pageSize);
}
