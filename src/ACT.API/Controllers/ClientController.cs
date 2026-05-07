using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using ACT.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ACT.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientController(IClientService clientService)
    {
        _clientService = clientService;
    }

    private int CompanyId => int.Parse(User.FindFirstValue("companyId")!);

    [HttpPost]
    public async Task<ActionResult> Create(Client client)
    {
        var createdClient = await _clientService.CreateAsync(CompanyId, client);
        return CreatedAtAction(nameof(GetPaged), new { id = createdClient.Id }, createdClient);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] Client updatedClient)
    {
        try
        {
            await _clientService.UpdateAsync(id, updatedClient);
            // Fetch the updated client to return
            var client = await _clientService.GetByIdAsync(id);
            return Ok(client);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    // GET /api/client/paged?page=1&pageSize=20
    [HttpGet("paged")]
    public async Task<ActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _clientService.GetPagedAsync(CompanyId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClientDto>> GetById(int id)
    {
        var client = await _clientService.GetByIdAsync(id);
        if (client == null) return NotFound();
        return Ok(client);
    }
}
