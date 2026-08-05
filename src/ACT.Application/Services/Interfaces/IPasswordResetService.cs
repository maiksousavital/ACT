namespace ACT.Application.Services.Interfaces;

public interface IPasswordResetService
{
    /// <summary>
    /// Always succeeds from the caller's point of view, whether or not the email is registered —
    /// the controller returns the same generic response either way so this can't be used to
    /// enumerate valid accounts.
    /// </summary>
    Task RequestResetAsync(string email);

    /// <summary>Throws ArgumentException (mapped to 400) for an invalid, expired, or already-used token.</summary>
    Task ResetPasswordAsync(string token, string newPassword);
}
