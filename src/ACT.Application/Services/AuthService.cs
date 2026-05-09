using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using ACT.Domain.Entities;
using ACT.Domain.Interfaces;

namespace ACT.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILoginHistoryRepository _loginHistory;

    public AuthService(
        IUserRepository userRepository,
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        ILoginHistoryRepository loginHistory)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _loginHistory = loginHistory;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequest request, string? ipAddress = null, string? userAgent = null)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
        {
            await RecordLogin(null, request.Email, null, ipAddress, userAgent, false, "User not found");
            return null;
        }

        if (!user.IsActive)
        {
            await RecordLogin(user.Id, user.Email, user.CompanyId, ipAddress, userAgent, false, "Account inactive");
            return null;
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            await RecordLogin(user.Id, user.Email, user.CompanyId, ipAddress, userAgent, false, "Invalid password");
            return null;
        }

        await RecordLogin(user.Id, user.Email, user.CompanyId, ipAddress, userAgent, true, null);

        var token = _jwtService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            Role = user.Role.ToString(),
            CompanyId = user.CompanyId
        };
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email);
        if (existing != null)
            throw new InvalidOperationException("A user with this email already exists.");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            CompanyId = request.CompanyId,
            Role = request.Role,
            IsActive = true
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            Role = user.Role.ToString(),
            CompanyId = user.CompanyId
        };
    }

    private async Task RecordLogin(int? userId, string email, int? companyId, string? ipAddress, string? userAgent, bool success, string? failureReason)
    {
        var record = new LoginHistory
        {
            UserId = userId,
            Email = email,
            CompanyId = companyId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Success = success,
            FailureReason = failureReason
        };
        await _loginHistory.AddAsync(record);
        await _loginHistory.SaveChangesAsync();
    }
}
