using ACT.Domain.Entities;
using ACT.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ACT.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    private readonly IClientRepository _clientRepository;

    public ClientController(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Client>>> GetAll(bool includeArchived = false)
    {
        var clients = await _clientRepository.GetAllAsync(includeArchived);
        return Ok(clients);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Client>> GetById(Guid id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client == null) return NotFound();
        return Ok(client);
    }

    [HttpPost]
    public async Task<ActionResult> Create(Client client)
    {
        await _clientRepository.AddAsync(client);
        await _clientRepository.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
    }
}

