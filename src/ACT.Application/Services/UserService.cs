using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using ACT.Domain.Entities;
using ACT.Domain.Interfaces;

namespace ACT.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(ToDto);
    }

    public async Task<IEnumerable<UserDto>> GetByCompanyAsync(int companyId)
    {
        var users = await _userRepository.GetByCompanyAsync(companyId);
        return users.Select(ToDto);
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? null : ToDto(user);
    }

    public async Task<UserDto> CreateAsync(RegisterRequest request)
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
        return ToDto(user);
    }

    public async Task<UserDto?> UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        if (!string.IsNullOrWhiteSpace(request.Email))
            user.Email = request.Email;
        if (request.CompanyId.HasValue && request.CompanyId.Value != user.CompanyId)
        {
            user.CompanyId = request.CompanyId.Value;
            user.TokenVersion++; // stale "companyId" claim on any already-issued token
        }
        if (request.Role.HasValue && request.Role.Value != user.Role)
        {
            user.Role = request.Role.Value;
            user.TokenVersion++; // stale "role" claim on any already-issued token
        }
        if (request.IsActive.HasValue && request.IsActive.Value != user.IsActive)
        {
            user.IsActive = request.IsActive.Value;
            user.TokenVersion++;
        }

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
        return ToDto(user);
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;

        user.IsActive = false;
        user.TokenVersion++;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    private static UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            CompanyId = user.CompanyId,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}

