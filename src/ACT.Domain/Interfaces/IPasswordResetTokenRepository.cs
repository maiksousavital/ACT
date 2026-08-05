using ACT.Domain.Entities;

namespace ACT.Domain.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task AddAsync(PasswordResetToken token);
    Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash);
    Task<IEnumerable<PasswordResetToken>> GetActiveByUserIdAsync(int userId);
    Task SaveChangesAsync();
}
