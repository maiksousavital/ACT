using ACT.Application.Dtos;

namespace ACT.Application.Services.Interfaces;

public interface IClientService
{
    Task<IEnumerable<ClientDto>> GetAllAsync(int? companyId, bool includeDeleted = false);
    Task<ClientDto?> GetByIdAsync(int id);
    Task<ClientDto> CreateAsync(int companyId, CreateClientRequest request, int? userId, string userEmail);
    Task<ClientDto?> UpdateAsync(int id, UpdateClientRequest request, int? userId, string userEmail);
    Task<PagedResult<ClientDto>> GetPagedAsync(int? companyId, int page, int pageSize);
    Task<bool> DeleteAsync(int id, int? userId, string userEmail);
}
