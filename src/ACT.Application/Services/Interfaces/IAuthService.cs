using ACT.Application.Dtos;

namespace ACT.Application.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginRequest request, string? ipAddress = null, string? userAgent = null);
    Task<AuthResponseDto> RegisterAsync(RegisterRequest request);

    /// <summary>Bumps the user's TokenVersion, revoking this and every other outstanding token for them.</summary>
    Task LogoutAsync(int userId);
}
