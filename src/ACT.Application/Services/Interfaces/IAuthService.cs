using ACT.Application.Dtos;

namespace ACT.Application.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginRequest request, string? ipAddress = null, string? userAgent = null);
    Task<AuthResponseDto> RegisterAsync(RegisterRequest request);
}
