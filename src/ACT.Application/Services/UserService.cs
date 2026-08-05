using ACT.Application.Common;
using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using ACT.Domain.Entities;
using ACT.Domain.Interfaces;

namespace ACT.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;

    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, IAuditService auditService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
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

    public async Task<UserDto> CreateAsync(RegisterRequest request, int? userId, string userEmail)
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
        await _auditService.LogAsync(userId, userEmail, user.CompanyId, "Create", "User", user.Id);
        return ToDto(user);
    }

    public async Task<UserDto?> UpdateAsync(int id, UpdateUserRequest request, int? userId, string userEmail)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        var newEmail = !string.IsNullOrWhiteSpace(request.Email) ? request.Email : user.Email;
        var newCompanyId = request.CompanyId.HasValue ? request.CompanyId.Value : (int?)user.CompanyId;
        var newRole = request.Role ?? user.Role;
        var newIsActive = request.IsActive ?? user.IsActive;

        var changes = AuditDiff.Compare(
            ("Email", user.Email, newEmail),
            ("CompanyId", user.CompanyId, newCompanyId),
            ("Role", user.Role, newRole),
            ("IsActive", user.IsActive, newIsActive)
        );

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
        await _auditService.LogChangesAsync(userId, userEmail, user.CompanyId, "User", id, changes);
        return ToDto(user);
    }

    public async Task<bool> DeactivateAsync(int id, int? userId, string userEmail)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;

        user.IsActive = false;
        user.TokenVersion++;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
        await _auditService.LogAsync(userId, userEmail, user.CompanyId, "Deactivate", "User", user.Id);
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

