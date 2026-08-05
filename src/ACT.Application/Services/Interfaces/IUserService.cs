using ACT.Application.Dtos;

namespace ACT.Application.Services.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<IEnumerable<UserDto>> GetByCompanyAsync(int companyId);
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto> CreateAsync(RegisterRequest request, int? userId, string userEmail);
    Task<UserDto?> UpdateAsync(int id, UpdateUserRequest request, int? userId, string userEmail);
    Task<bool> DeactivateAsync(int id, int? userId, string userEmail);
}

