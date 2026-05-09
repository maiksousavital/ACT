using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ACT.API.Controllers.Admin;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
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
    /// GET /api/user — list users for the authenticated user's company.
    /// SuperAdmin can pass ?companyId=X to list any company's users.
    /// </summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetByCompany([FromQuery] int? companyId = null)
    {
        var targetCompanyId = companyId ?? CompanyId;
        if (targetCompanyId == null)
            return BadRequest(new { message = "companyId query param is required for SuperAdmin." });

        // Non-SuperAdmin can only list their own company's users
        if (!IsSuperAdmin && targetCompanyId != CompanyId)
            return Forbid();

        var users = await _userService.GetByCompanyAsync(targetCompanyId.Value);
        return Ok(users);
    }

    /// <summary>
    /// GET /api/user/{id} — get a single user.
    /// </summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();

        // Non-SuperAdmin can only see users in their own company
        if (!IsSuperAdmin && user.CompanyId != CompanyId)
            return Forbid();

        return Ok(user);
    }

    /// <summary>
    /// POST /api/user — create a new user for a company.
    /// Admin creates users for their own company. SuperAdmin can create for any company.
    /// </summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] RegisterRequest request)
    {
        // Admin can only create users for their own company
        if (!IsSuperAdmin)
        {
            if (CompanyId == null || request.CompanyId != CompanyId.Value)
                return Forbid();

            // Admin cannot create SuperAdmin or Admin users
            if (request.Role == Domain.Enums.Role.SuperAdmin)
                return Forbid();
        }

        try
        {
            var user = await _userService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// PUT /api/user/{id} — update user role or active status.
    /// </summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UpdateUserRequest request)
    {
        var existingUser = await _userService.GetByIdAsync(id);
        if (existingUser == null) return NotFound();

        // Non-SuperAdmin can only update users in their own company
        if (!IsSuperAdmin && existingUser.CompanyId != CompanyId)
            return Forbid();

        // Admin cannot promote to SuperAdmin
        if (!IsSuperAdmin && request.Role == Domain.Enums.Role.SuperAdmin)
            return Forbid();

        var updated = await _userService.UpdateAsync(id, request);
        return Ok(updated);
    }

    /// <summary>
    /// DELETE /api/user/{id} — deactivate a user (soft delete).
    /// </summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Deactivate(int id)
    {
        var existingUser = await _userService.GetByIdAsync(id);
        if (existingUser == null) return NotFound();

        // Non-SuperAdmin can only deactivate users in their own company
        if (!IsSuperAdmin && existingUser.CompanyId != CompanyId)
            return Forbid();

        await _userService.DeactivateAsync(id);
        return NoContent();
    }
}

