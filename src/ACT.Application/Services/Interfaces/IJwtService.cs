using ACT.Domain.Entities;

namespace ACT.Application.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}

