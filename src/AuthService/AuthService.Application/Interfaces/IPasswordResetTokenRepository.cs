using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken> CreateAsync(PasswordResetToken token);
    Task<PasswordResetToken?> GetByTokenAsync(string token);
    Task UpdateAsync(PasswordResetToken token);
    Task DeleteExpiredTokensAsync();
}
