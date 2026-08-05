namespace ACT.Domain.Entities;

/// <summary>
/// Only the SHA-256 hash of the reset token is ever stored — the raw token exists only in the
/// email sent to the user and briefly in memory while handling the request, the same principle
/// as password hashing. A leaked database can't be used to forge a reset link.
/// </summary>
public class PasswordResetToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Null until consumed. A used token is never valid again, even if not yet expired.</summary>
    public DateTime? UsedAt { get; set; }
}
