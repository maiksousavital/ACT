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
public class TreatmentTypeController : ControllerBase
{
    private readonly ITreatmentTypeService _treatmentTypeService;

    public TreatmentTypeController(ITreatmentTypeService treatmentTypeService)
    {
        _treatmentTypeService = treatmentTypeService;
    }

    private int CompanyId => int.Parse(User.FindFirstValue("companyId")!);

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
        var created = await _treatmentTypeService.CreateAsync(CompanyId, request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TreatmentTypeDto>> Update(int id, [FromBody] UpdateTreatmentTypeRequest request)
    {
        var updated = await _treatmentTypeService.UpdateAsync(id, request);
        if (updated == null) return NotFound();
        return Ok(updated);
    }
}
