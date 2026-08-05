using ACT.API.Extensions;
using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using ACT.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ACT.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TreatmentTypeController : ControllerBase
{
    private readonly ITreatmentTypeService _treatmentTypeService;

    public TreatmentTypeController(ITreatmentTypeService treatmentTypeService)
    {
        _treatmentTypeService = treatmentTypeService;
    }

    private int? CompanyId => User.GetCompanyId();

    private bool IsSuperAdmin => User.IsSuperAdmin();

    // GET /api/treatmenttype/paged?page=1&pageSize=20
    [HttpGet("paged")]
    public async Task<ActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _treatmentTypeService.GetPagedAsync(CompanyId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TreatmentTypeDto>> GetById(int id)
    {
        var type = await _treatmentTypeService.GetByIdAsync(id);
        if (type == null) return NotFound();

        // Non-SuperAdmin can only view treatment types belonging to their own company
        if (!IsSuperAdmin && type.CompanyId != CompanyId)
            return Forbid();

        return Ok(type);
    }

    [HttpGet("add-edit-metadata")]
    public ActionResult GetAddEditMetadata()
    {
        return Ok(new {
            allowedFollowUpPeriods = TreatmentTypeDto.AllowedFollowUpPeriods
        });
    }

    [HttpPost]
    public async Task<ActionResult<TreatmentTypeDto>> Create([FromBody] CreateTreatmentTypeRequest request)
    {
        if (CompanyId == null)
            return BadRequest(new { message = "companyId is required. SuperAdmin must specify a company." });
        var created = await _treatmentTypeService.CreateAsync(CompanyId.Value, request, User.GetUserId(), User.GetEmail());
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TreatmentTypeDto>> Update(int id, [FromBody] UpdateTreatmentTypeRequest request)
    {
        var existing = await _treatmentTypeService.GetByIdAsync(id);
        if (existing == null) return NotFound();

        // Non-SuperAdmin can only update treatment types belonging to their own company
        if (!IsSuperAdmin && existing.CompanyId != CompanyId)
            return Forbid();

        var updated = await _treatmentTypeService.UpdateAsync(id, request, User.GetUserId(), User.GetEmail());
        if (updated == null) return NotFound();
        return Ok(updated);
    }
}
