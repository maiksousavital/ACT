using ACT.API.Extensions;
using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ACT.API.Controllers;

[Authorize]
[ApiController]
[Route("api/treatment")]
public class TreatmentsController : ControllerBase
{
    private readonly ITreatmentService _service;

    public TreatmentsController(ITreatmentService service)
    {
        _service = service;
    }

    private int? CompanyId => User.GetCompanyId();

    private bool IsSuperAdmin => User.IsSuperAdmin();

    // POST /api/treatments
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTreatmentRequest request)
    {
        if (CompanyId == null)
            return BadRequest(new { message = "companyId is required. SuperAdmin must specify a company." });
        // A cross-company ClientId/TreatmentTypeId throws KeyNotFoundException, mapped to 404
        // by GlobalExceptionHandler (see A4) — no local catch needed.
        var result = await _service.CreateAsync(CompanyId.Value, request);
        return CreatedAtAction(nameof(GetPaged),
            new { clientId = result.ClientId }, result);
    }

    // GET /api/treatments/paged?page=1&pageSize=20
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetPagedAsync(CompanyId, page, pageSize);
        return Ok(result);
    }

    // PUT /api/treatment/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTreatmentRequest request)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound();

        // Non-SuperAdmin can only update treatments belonging to their own company
        if (!IsSuperAdmin && existing.CompanyId != CompanyId)
            return Forbid();

        // A cross-company ClientId/TreatmentTypeId reassignment throws KeyNotFoundException,
        // mapped to 404 by GlobalExceptionHandler (see A4) — no local catch needed.
        var updated = await _service.UpdateAsync(id, request);
        if (updated == null)
            return NotFound();
        return Ok(updated);
    }

    // GET /api/treatment/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TreatmentDto>> GetById(int id)
    {
        var treatment = await _service.GetByIdAsync(id);
        if (treatment == null) return NotFound();

        // Non-SuperAdmin can only view treatments belonging to their own company
        if (!IsSuperAdmin && treatment.CompanyId != CompanyId)
            return Forbid();

        return Ok(treatment);
    }

    // GET /api/treatment/by-client/{clientId}
    [HttpGet("by-client/{clientId}")]
    public async Task<IActionResult> GetByClient(int clientId)
    {
        var treatments = await _service.GetByClientAsync(clientId, CompanyId);
        return Ok(treatments);
    }
}
