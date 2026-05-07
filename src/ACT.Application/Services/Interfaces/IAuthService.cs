using ACT.Application.Dtos;

namespace ACT.Application.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginRequest request);
    Task<AuthResponseDto> RegisterAsync(RegisterRequest request);
}

