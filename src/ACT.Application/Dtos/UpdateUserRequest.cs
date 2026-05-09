using ACT.Domain.Enums;

namespace ACT.Application.Dtos;

public class UpdateUserRequest
{
    public Role? Role { get; set; }
    public bool? IsActive { get; set; }
}

