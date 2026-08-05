using System.ComponentModel.DataAnnotations;

namespace ACT.Application.Dtos;

public class ForgotPasswordRequest
{
    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;
}
