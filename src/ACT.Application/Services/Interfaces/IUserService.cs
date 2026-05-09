using ACT.Application.Dtos;

namespace ACT.Application.Services.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetByCompanyAsync(int companyId);
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto> CreateAsync(RegisterRequest request);
    Task<UserDto?> UpdateAsync(int id, UpdateUserRequest request);
    Task<bool> DeactivateAsync(int id);
}

