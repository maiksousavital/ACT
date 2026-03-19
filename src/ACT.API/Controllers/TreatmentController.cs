
using ACT.Application.Dtos;
using ACT.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ACT.API.Controllers;

[ApiController]
[Route("api/treatment")]
public class TreatmentsController : ControllerBase
{
    private readonly TreatmentService _service;

    public TreatmentsController(TreatmentService service)
    {
        _service = service;
    }

    // GET /api/treatments/client/{clientId}
    [HttpGet("client/{clientId:guid}")]
    public async Task<IActionResult> GetByClient(Guid clientId)
    {
        var result = await _service.GetByClientAsync(clientId);
        return Ok(result);
    }

    // POST /api/treatments
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTreatmentRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetByClient),
                new { clientId = result.ClientId }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

/*
using ACT.Domain.Entities;
using ACT.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ACT.Application.Dtos;

namespace ACT.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TreatmentController : ControllerBase
{
    private readonly ITreatmentRepository _treatmentRepository;
    private readonly ITreatmentTypeRepository _treatmentTypeRepository;

    public TreatmentController(ITreatmentRepository treatmentRepository, ITreatmentTypeRepository treatmentTypeRepository)
    {
        _treatmentRepository = treatmentRepository;
        _treatmentTypeRepository = treatmentTypeRepository;
    }

    [HttpGet("due")]
    public async Task<ActionResult<IEnumerable<TreatmentDto>>> GetDue()
    {
        var treatments = await _treatmentRepository.GetDueAsync();
        var dtos = treatments.Select(TreatmentDto.FromEntity);
        return Ok(dtos);
    }

    [HttpGet("by-client/{clientId}")]
    public async Task<ActionResult<IEnumerable<TreatmentDto>>> GetByClient(Guid clientId)
    {
        var treatments = await _treatmentRepository.GetByClientAsync(clientId);
        var dtos = treatments.Select(TreatmentDto.FromEntity);
        return Ok(dtos);
    }

    [HttpPost]
    public async Task<ActionResult<TreatmentDto>> Create([FromBody] CreateTreatmentRequest request)
    {
        var treatmentType = await _treatmentTypeRepository.GetByIdAsync(request.TreatmentTypeId);
        if (treatmentType == null)
        {
            return BadRequest("Invalid treatment type.");
        }
        var treatment = Treatment.Create(request.ClientId, treatmentType, request.TreatmentDate, request.Notes);
        await _treatmentRepository.AddAsync(treatment);
        await _treatmentRepository.SaveChangesAsync();
        return Ok(TreatmentDto.FromEntity(treatment));
    }
}

/*
public class CreateTreatmentRequest
{
    public Guid ClientId { get; set; }
    public Guid TreatmentTypeId { get; set; }
    public DateTime TreatmentDate { get; set; }
    public string? Notes { get; set; }
}
#1#

/*public class TreatmentDto
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid TreatmentTypeId { get; set; }
    public DateTime TreatmentDate { get; set; }
    public DateTime NextFollowUpDate { get; set; }
    public string? Notes { get; set; }
    public DateTime? FollowedUpAt { get; set; }
    public string? FollowUpNotes { get; set; }
    public bool IsFollowedUp { get; set; }
    public bool IsDue { get; set; }

    public static TreatmentDto FromEntity(Treatment t) => new TreatmentDto
    {
        Id = t.Id,
        ClientId = t.ClientId,
        TreatmentTypeId = t.TreatmentTypeId,
        TreatmentDate = t.TreatmentDate,
        NextFollowUpDate = t.NextFollowUpDate,
        Notes = t.Notes,
        FollowedUpAt = t.FollowedUpAt,
        FollowUpNotes = t.FollowUpNotes,
        IsFollowedUp = t.IsFollowedUp,
        IsDue = t.IsDue
    };
}#1#
*/
