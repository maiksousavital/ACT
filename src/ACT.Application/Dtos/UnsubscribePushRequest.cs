using System.ComponentModel.DataAnnotations;

namespace ACT.Application.Dtos;

public class UnsubscribePushRequest
{
    [Required, MaxLength(500)]
    public string Endpoint { get; set; } = string.Empty;
}
