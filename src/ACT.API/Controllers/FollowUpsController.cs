using ACT.API.Extensions;
using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ACT.API.Controllers;

[Authorize]
[ApiController]
[Route("api/followups")]
public class FollowUpsController : ControllerBase
{
    private readonly ITreatmentService _service;

    public FollowUpsController(ITreatmentService service)
    {
        _service = service;
    }

    private int? CompanyId => User.GetCompanyId();

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
        // Unknown id -> KeyNotFoundException (404); already-followed-up -> InvalidOperationException
        // (409) — both mapped by GlobalExceptionHandler (see A4), no local catch needed.
        var result = await _service.CompleteFollowUpAsync(id, request, User.GetUserId(), User.GetEmail());
        return Ok(result);
    }
}