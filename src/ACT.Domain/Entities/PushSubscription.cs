namespace ACT.Domain.Entities;

/// <summary>
/// One row per browser/device a user has granted Web Push permission on — a user can have several
/// (phone + laptop). Endpoint/P256dh/Auth are exactly what the browser's PushManager.subscribe()
/// returns; they're opaque to us and only ever handed to the push service that issued them.
/// </summary>
public class PushSubscription
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
