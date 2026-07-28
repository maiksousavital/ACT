using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ACT.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Public — authenticate with email and password, returns JWT.
    /// </summary>
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers["User-Agent"].FirstOrDefault();
        var result = await _authService.LoginAsync(request, ip, userAgent);
        if (result == null)
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(result);
    }

    /// <summary>
    /// SuperAdmin only — create a new user for a company.
    /// </summary>
    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return Created("", result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Revokes the caller's token (and every other outstanding token for the same account) by
    /// bumping their TokenVersion — the next authenticated request on any old token gets 401.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        await _authService.LogoutAsync(userId);
        return NoContent();
    }
}

