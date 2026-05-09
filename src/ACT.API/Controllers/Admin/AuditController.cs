using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ACT.API.Controllers.Admin;

[Authorize(Roles = "SuperAdmin,Admin")]
[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    private int? CompanyId
    {
        get
        {
            var claim = User.FindFirstValue("companyId");
            return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
        }
    }

    private bool IsSuperAdmin => User.FindFirstValue("role") == "SuperAdmin";

    /// <summary>
    /// GET /api/audit/logs — paginated audit trail.
    /// SuperAdmin sees all; Admin sees own company only.
    /// </summary>
    [HttpGet("logs")]
    public async Task<ActionResult> GetAuditLogs(
        [FromQuery] int? companyId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var targetCompanyId = IsSuperAdmin ? companyId : CompanyId;
        var result = await _auditService.GetPagedAsync(targetCompanyId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/audit/logins — paginated login history.
    /// SuperAdmin sees all; Admin sees own company only.
    /// </summary>
    [HttpGet("logins")]
    public async Task<ActionResult> GetLoginHistory(
        [FromQuery] int? companyId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var targetCompanyId = IsSuperAdmin ? companyId : CompanyId;
        var result = await _auditService.GetLoginHistoryPagedAsync(targetCompanyId, page, pageSize);
        return Ok(result);
    }
}

