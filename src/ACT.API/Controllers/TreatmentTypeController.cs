using ACT.Domain.Entities;
using ACT.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ACT.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TreatmentTypeController : ControllerBase
{
    private readonly ITreatmentTypeRepository _treatmentTypeRepository;

    public TreatmentTypeController(ITreatmentTypeRepository treatmentTypeRepository)
    {
        _treatmentTypeRepository = treatmentTypeRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TreatmentType>>> GetAllActive()
    {
        var types = await _treatmentTypeRepository.GetAllActiveAsync();
        return Ok(types);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TreatmentType>> GetById(Guid id)
    {
        var type = await _treatmentTypeRepository.GetByIdAsync(id);
        if (type == null) return NotFound();
        return Ok(type);
    }

    [HttpPost]
    public async Task<ActionResult> Create(TreatmentType treatmentType)
    {
        await _treatmentTypeRepository.AddAsync(treatmentType);
        await _treatmentTypeRepository.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = treatmentType.Id }, treatmentType);
    }
}

