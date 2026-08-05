using System.Security.Cryptography;
using System.Text;
using ACT.Application.Services.Interfaces;
using ACT.Domain.Entities;
using ACT.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ACT.Application.Services;

public class PasswordResetService : IPasswordResetService
{
    private const int TokenValidityMinutes = 30;

    private readonly IUserRepository _users;
    private readonly IPasswordResetTokenRepository _tokens;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailSender _emailSender;
    private readonly IAuditService _auditService;
    private readonly IConfiguration _config;

    public PasswordResetService(
        IUserRepository users,
        IPasswordResetTokenRepository tokens,
        IPasswordHasher passwordHasher,
        IEmailSender emailSender,
        IAuditService auditService,
        IConfiguration config)
    {
        _users = users;
        _tokens = tokens;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
        _auditService = auditService;
        _config = config;
    }

    public async Task RequestResetAsync(string email)
    {
        var user = await _users.GetByEmailAsync(email);

        // Deliberately no-op — not an exception — when the account doesn't exist or is inactive.
        // The controller returns the identical response either way, so there's nothing here that
        // should behave differently based on account existence.
        if (user == null || !user.IsActive)
            return;

        // At most one live token per user: burn anything still outstanding before issuing a new one.
        var stillActive = await _tokens.GetActiveByUserIdAsync(user.Id);
        foreach (var old in stillActive)
            old.UsedAt = DateTime.UtcNow;

        var rawToken = GenerateToken();
        await _tokens.AddAsync(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = Hash(rawToken),
            ExpiresAt = DateTime.UtcNow.AddMinutes(TokenValidityMinutes)
        });
        await _tokens.SaveChangesAsync();

        var frontendBaseUrl = (_config["Frontend:BaseUrl"] ?? "http://localhost:5173").TrimEnd('/');
        var resetLink = $"{frontendBaseUrl}/reset-password?token={Uri.EscapeDataString(rawToken)}";
        var body = $"""
            <p>A password reset was requested for your ACT account.</p>
            <p><a href="{resetLink}">Reset your password</a></p>
            <p>This link expires in {TokenValidityMinutes} minutes. If you didn't request this, you can safely ignore this email — your password hasn't been changed.</p>
            """;
        await _emailSender.SendAsync(user.Email, "Reset your ACT password", body);

        await _auditService.LogAsync(user.Id, user.Email, user.CompanyId, "PasswordResetRequested", "User", user.Id);
    }

    public async Task ResetPasswordAsync(string token, string newPassword)
    {
        var entity = await _tokens.GetByTokenHashAsync(Hash(token));

        if (entity == null || entity.UsedAt != null || entity.ExpiresAt < DateTime.UtcNow)
            throw new ArgumentException("This reset link is invalid or has expired.");

        var user = await _users.GetByIdAsync(entity.UserId)
            ?? throw new ArgumentException("This reset link is invalid or has expired.");

        user.PasswordHash = _passwordHasher.Hash(newPassword);
        user.TokenVersion++; // revokes every already-issued session for this user — see User.TokenVersion
        await _users.UpdateAsync(user);

        entity.UsedAt = DateTime.UtcNow;

        // Defense in depth: also burn any other still-live token for this user (e.g. two reset
        // emails requested back to back before either was used).
        var stillActive = await _tokens.GetActiveByUserIdAsync(user.Id);
        foreach (var other in stillActive)
            if (other.Id != entity.Id) other.UsedAt = DateTime.UtcNow;

        await _tokens.SaveChangesAsync();
        await _users.SaveChangesAsync();

        await _auditService.LogAsync(user.Id, user.Email, user.CompanyId, "PasswordReset", "User", user.Id);
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        // base64url — the raw base64 alphabet includes '+' and '/', which need escaping in a URL;
        // this is safe to drop straight into a query string.
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
