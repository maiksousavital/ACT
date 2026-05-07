using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ACT.API.Controllers;

[ApiController]
[Route("api/followups")]
public class FollowUpsController : ControllerBase
{
    private readonly ITreatmentService _service;

    public FollowUpsController(ITreatmentService service)
    {
        _service = service;
    }

    // TODO: Phase 1.4 — resolve companyId from middleware instead of hardcoding
    private int CompanyId => int.TryParse(HttpContext.Items["CompanyId"]?.ToString(), out var id) ? id : 1;

    // GET /api/followups/due
    // All outstanding follow-ups — used by the Treatments page
    [HttpGet("due")]
    public async Task<IActionResult> GetDue()
    {
        var result = await _service.GetDueAsync(CompanyId);
        return Ok(result);
    }

    // GET /api/followups/today
    // Only today's follow-ups — used by the Dashboard
    [HttpGet("today")]
    public async Task<IActionResult> GetToday()
    {
        var result = await _service.GetTodayAsync(CompanyId);
        return Ok(result);
    }

    // POST /api/followups/{id}/complete
    // Marks a follow-up done and schedules the next one automatically
    [HttpPost("{id:int}/complete")]
    public async Task<IActionResult> Complete(
        int id,
        [FromBody] CompleteFollowUpRequest request)
    {
        try
        {
            var result = await _service.CompleteFollowUpAsync(id, request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}