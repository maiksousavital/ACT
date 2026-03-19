using ACT.Application.Dtos;
using ACT.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ACT.API.Controllers;

[ApiController]
[Route("api/followups")]
public class FollowUpsController : ControllerBase
{
    private readonly TreatmentService _service;

    public FollowUpsController(TreatmentService service)
    {
        _service = service;
    }

    // GET /api/followups/due
    // All outstanding follow-ups — used by the Treatments page
    [HttpGet("due")]
    public async Task<IActionResult> GetDue()
    {
        var result = await _service.GetDueAsync();
        return Ok(result);
    }

    // GET /api/followups/today
    // Only today's follow-ups — used by the Dashboard
    [HttpGet("today")]
    public async Task<IActionResult> GetToday()
    {
        var result = await _service.GetTodayAsync();
        return Ok(result);
    }

    // POST /api/followups/{id}/complete
    // Marks a follow-up done and schedules the next one automatically
    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(
        Guid id,
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