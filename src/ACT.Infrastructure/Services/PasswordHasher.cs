using ACT.Application.Services.Interfaces;

namespace ACT.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    // Explicit rather than relying on the library default, so a future library upgrade can't
    // silently change how expensive hashing is. 12 is comfortably above the recommended minimum.
    private const int WorkFactor = 12;

    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
    }

    public bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}

