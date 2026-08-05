using System.ComponentModel.DataAnnotations;

namespace ACT.Application.Dtos;

public class ResetPasswordRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(200)]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$",
        ErrorMessage = "Password must include at least one letter and one number.")]
    public string NewPassword { get; set; } = string.Empty;
}
