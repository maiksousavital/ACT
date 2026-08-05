using ACT.API.Extensions;
using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    private int? CompanyId => User.GetCompanyId();

    private bool IsSuperAdmin => User.IsSuperAdmin();

    [HttpPost]
    public async Task<ActionResult<ClientDto>> Create([FromBody] CreateClientRequest request)
    {
        if (CompanyId == null)
            return BadRequest(new { message = "companyId is required. SuperAdmin must specify a company." });
        var created = await _clientService.CreateAsync(CompanyId.Value, request, User.GetUserId(), User.GetEmail());
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ClientDto>> Update(int id, [FromBody] UpdateClientRequest request)
    {
        var existing = await _clientService.GetByIdAsync(id);
        if (existing == null) return NotFound();

        // Non-SuperAdmin can only update clients belonging to their own company
        if (!IsSuperAdmin && existing.CompanyId != CompanyId)
            return Forbid();

        var updated = await _clientService.UpdateAsync(id, request, User.GetUserId(), User.GetEmail());
        if (updated == null) return NotFound();
        return Ok(updated);
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

        // Non-SuperAdmin can only view clients belonging to their own company
        if (!IsSuperAdmin && client.CompanyId != CompanyId)
            return Forbid();

        return Ok(client);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var existing = await _clientService.GetByIdAsync(id);
        if (existing == null) return NotFound();

        // Non-SuperAdmin can only delete clients belonging to their own company
        if (!IsSuperAdmin && existing.CompanyId != CompanyId)
            return Forbid();

        var result = await _clientService.DeleteAsync(id, User.GetUserId(), User.GetEmail());
        if (!result) return NotFound();
        return NoContent();
    }
}
