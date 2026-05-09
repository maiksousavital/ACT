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

    public async Task<IEnumerable<ClientDto>> GetAllAsync(int? companyId, bool includeArchived = false)
    {
        var clients = await _clientRepository.GetAllAsync(companyId, includeArchived);
        return clients.Select(ToDto);
    }

    public async Task<ClientDto?> GetByIdAsync(int id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        return client == null ? null : ToDto(client);
    }

    public async Task<ClientDto> CreateAsync(int companyId, Client client)
    {
        client.CompanyId = companyId;
        await _clientRepository.AddAsync(client);
        await _clientRepository.SaveChangesAsync();
        return ToDto(client);
    }

    public async Task UpdateAsync(int id, Client updatedClient)
    {
        var existingClient = await _clientRepository.GetByIdAsync(id);
        if (existingClient == null)
            throw new Exception("Client not found");

        existingClient.FirstName = updatedClient.FirstName;
        existingClient.LastName = updatedClient.LastName;
        existingClient.Phone = updatedClient.Phone;
        existingClient.Email = updatedClient.Email;
        existingClient.Notes = updatedClient.Notes;
        existingClient.IsArchived = updatedClient.IsArchived;
        // Do not update CreatedAt or Id

        await _clientRepository.UpdateAsync(existingClient);
        await _clientRepository.SaveChangesAsync();
    }

    public async Task<PagedResult<ClientDto>> GetPagedAsync(int? companyId, int page, int pageSize)
    {
        var (items, totalCount) = await _clientRepository.GetPagedAsync(companyId, page, pageSize);
        return new PagedResult<ClientDto>
        {
            Items = items.Select(ToDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private static ClientDto ToDto(Client client)
    {
        return new ClientDto
        {
            Id = client.Id,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Phone = client.Phone,
            Email = client.Email,
            Notes = client.Notes,
            IsArchived = client.IsArchived,
            CreatedAt = client.CreatedAt
        };
    }
}
