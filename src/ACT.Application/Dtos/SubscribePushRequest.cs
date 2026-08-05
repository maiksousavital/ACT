using System.ComponentModel.DataAnnotations;

namespace ACT.Application.Dtos;

public class SubscribePushRequest
{
    [Required, MaxLength(500)]
    public string Endpoint { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string P256dh { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Auth { get; set; } = string.Empty;
}
