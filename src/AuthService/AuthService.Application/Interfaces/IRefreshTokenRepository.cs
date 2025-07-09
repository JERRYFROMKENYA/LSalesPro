using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<RefreshToken?> GetByIdAsync(Guid id);
    Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId);
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
    Task DeleteAsync(Guid id);
    Task DeleteByTokenAsync(string token);
    Task DeleteAllUserTokensAsync(Guid userId);
    Task DeleteExpiredTokensAsync();
    Task<bool> IsValidTokenAsync(string token);
}
