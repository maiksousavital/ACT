using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ACT.API.Controllers;

[ApiController]
[Route("api/treatment")]
public class TreatmentsController : ControllerBase
{
    private readonly ITreatmentService _service;

    public TreatmentsController(ITreatmentService service)
    {
        _service = service;
    }

    // TODO: Phase 1.4 — resolve companyId from middleware instead of hardcoding
    private int CompanyId => int.TryParse(HttpContext.Items["CompanyId"]?.ToString(), out var id) ? id : 1;

    // POST /api/treatments
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTreatmentRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(CompanyId, request);
            return CreatedAtAction(nameof(GetPaged),
                new { clientId = result.ClientId }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
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
        try
        {
            var updated = await _service.UpdateAsync(id, request);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/treatment/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TreatmentDto>> GetById(int id)
    {
        var treatment = await _service.GetByIdAsync(id);
        if (treatment == null) return NotFound();
        return Ok(treatment);
    }
}

