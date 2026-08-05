using ACT.API.Extensions;
using ACT.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ACT.API.Controllers;

/// <summary>
/// Serves the per-record "Change History" popup on an entity's edit screen. Unlike
/// ACT.API.Controllers.Admin.AuditController (Admin/SuperAdmin-only global log), this is scoped
/// to a single entity and open to any authenticated user who can view/edit that entity — same
/// company-scoping rule already used by ClientController etc.
/// </summary>
[Authorize]
[ApiController]
[Route("api/audit")]
public class EntityAuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public EntityAuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    private int? CompanyId => User.GetCompanyId();

    private bool IsSuperAdmin => User.IsSuperAdmin();

    // GET /api/audit/entity/{entityType}/{entityId}?search=&from=&to=&page=1&pageSize=20
    [HttpGet("entity/{entityType}/{entityId:int}")]
    public async Task<ActionResult> GetForEntity(
        string entityType,
        int entityId,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _auditService.GetForEntityPagedAsync(
            IsSuperAdmin ? null : CompanyId, entityType, entityId, search, from, to, page, pageSize);
        return Ok(result);
    }
}
