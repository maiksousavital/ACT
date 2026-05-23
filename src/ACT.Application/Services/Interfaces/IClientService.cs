using ACT.Application.Dtos;

namespace ACT.Application.Services.Interfaces;

public interface IClientService
{
    Task<IEnumerable<ClientDto>> GetAllAsync(int? companyId, bool includeArchived = false);
    Task<ClientDto?> GetByIdAsync(int id);
    Task<ClientDto> CreateAsync(int companyId, CreateClientRequest request);
    Task<ClientDto?> UpdateAsync(int id, UpdateClientRequest request);
    Task<PagedResult<ClientDto>> GetPagedAsync(int? companyId, int page, int pageSize);
}
