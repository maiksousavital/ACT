using ACT.Application.Common;
using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using ACT.Domain.Entities;
using ACT.Domain.Interfaces;

namespace ACT.Application.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;

    public ClientService(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<IEnumerable<ClientDto>> GetAllAsync(int? companyId, bool includeDeleted = false)
    {
        var clients = await _clientRepository.GetAllAsync(companyId, includeDeleted);
        return clients.Select(ToDto);
    }

    public async Task<ClientDto?> GetByIdAsync(int id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        return client == null ? null : ToDto(client);
    }

    public async Task<ClientDto> CreateAsync(int companyId, CreateClientRequest request)
    {
        var client = new Client
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Notes = request.Notes,
            CompanyId = companyId
        };
        await _clientRepository.AddAsync(client);
        await _clientRepository.SaveChangesAsync();
        return ToDto(client);
    }

    public async Task<ClientDto?> UpdateAsync(int id, UpdateClientRequest request)
    {
        var existingClient = await _clientRepository.GetByIdAsync(id);
        if (existingClient == null)
            return null;

        existingClient.FirstName = request.FirstName;
        existingClient.LastName = request.LastName;
        existingClient.Phone = request.Phone;
        existingClient.Email = request.Email;
        existingClient.Notes = request.Notes;

        await _clientRepository.UpdateAsync(existingClient);
        await _clientRepository.SaveChangesAsync();
        return ToDto(existingClient);
    }

    public async Task<PagedResult<ClientDto>> GetPagedAsync(int? companyId, int page, int pageSize)
    {
        (page, pageSize) = Paging.Clamp(page, pageSize);
        var (items, totalCount) = await _clientRepository.GetPagedAsync(companyId, page, pageSize);
        return new PagedResult<ClientDto>
        {
            Items = items.Select(ToDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client == null) return false;

        client.IsDeleted = true;
        await _clientRepository.UpdateAsync(client);
        await _clientRepository.SaveChangesAsync();
        return true;
    }

    private static ClientDto ToDto(Client client)
    {
        return new ClientDto
        {
            Id = client.Id,
            CompanyId = client.CompanyId,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Phone = client.Phone,
            Email = client.Email,
            Notes = client.Notes,
            IsDeleted = client.IsDeleted,
            CreatedAt = client.CreatedAt
        };
    }
}
