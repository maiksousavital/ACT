using ACT.Application.Services.Interfaces;
using ACT.Domain.Entities;
using ACT.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ACT.Infrastructure.Persistence;

/// <summary>
/// Ensures a SuperAdmin account exists without ever baking a known password into source control.
/// Runs once at startup, after migrations: no-ops if a SuperAdmin already exists.
/// </summary>
public static class AdminSeeder
{
    public static async Task SeedSuperAdminAsync(AppDbContext db, IPasswordHasher hasher, ILogger logger)
    {
        if (await db.Users.AnyAsync(u => u.Role == Role.SuperAdmin))
            return;

        var email = Environment.GetEnvironmentVariable("SEED_ADMIN_EMAIL") ?? "admin@act.local";
        var envPassword = Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD");
        var generated = string.IsNullOrWhiteSpace(envPassword);
        var password = generated ? GenerateRandomPassword() : envPassword!;

        var user = new User
        {
            Email = email,
            PasswordHash = hasher.Hash(password),
            CompanyId = null,
            Role = Role.SuperAdmin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        if (generated)
        {
            logger.LogWarning(
                "No SuperAdmin account existed — created one with a randomly generated password. " +
                "This password is logged ONCE and stored nowhere else: Email={Email} Password={Password} " +
                "Save it now and change it after first login. Set SEED_ADMIN_PASSWORD to control this instead.",
                email, password);
        }
        else
        {
            logger.LogInformation("Seeded SuperAdmin {Email} using SEED_ADMIN_PASSWORD.", email);
        }
    }

    private static string GenerateRandomPassword()
    {
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(18);
        // Base64 can omit digits/symbols entirely depending on random bytes; append a fixed
        // complexity suffix so the result always clears typical password-policy checks.
        return Convert.ToBase64String(bytes) + "!Aa1";
    }
}
