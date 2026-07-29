using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ACT.API.Extensions;
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
    private const string CookieName = "act_token";

    private readonly IAuthService _authService;
    private readonly IConfiguration _config;

    public AuthController(IAuthService authService, IConfiguration config)
    {
        _authService = authService;
        _config = config;
    }

    /// <summary>
    /// Public — authenticate with email and password. The JWT is set as an httpOnly cookie —
    /// never exposed to JS, closing the XSS-token-theft vector localStorage had — instead of
    /// being returned in the response body.
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

        SetAuthCookie(result.Token);
        return Ok(new { result.Email, result.Role, result.CompanyId });
    }

    /// <summary>
    /// SuperAdmin only — create a new user for a company. Returns the new account's details, not
    /// a live token — the caller (a SuperAdmin) isn't meant to end up signed in as the new user.
    /// </summary>
    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // A duplicate email throws InvalidOperationException, mapped to 409 by
        // GlobalExceptionHandler (see A4) — no local catch needed.
        var result = await _authService.RegisterAsync(request);
        return Created("", new { result.Email, result.Role, result.CompanyId });
    }

    /// <summary>
    /// Returns the current caller's identity. Since the JWT is httpOnly and invisible to JS, the
    /// SPA calls this (the cookie is sent automatically) to restore auth state on page load
    /// instead of decoding the token client-side.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            Email = User.FindFirstValue("email"),
            Role = User.FindFirstValue("role"),
            CompanyId = User.GetCompanyId()
        });
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
        Response.Cookies.Delete(CookieName, new CookieOptions { Path = "/" });
        return NoContent();
    }

    private void SetAuthCookie(string token)
    {
        var expiryMinutes = int.Parse(_config["JwtSettings:ExpiryMinutes"]!);
        Response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes),
            Path = "/"
        });
    }
}

