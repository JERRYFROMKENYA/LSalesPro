using System.Security.Claims;
using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IJwtTokenService
{
    Task<string> GenerateAccessTokenAsync(User user);
    Task<RefreshToken> GenerateRefreshTokenAsync(User user);
    bool ValidateToken(string token);
    Task<ClaimsPrincipal?> ValidateTokenAsync(string token);
    
    Task<bool> ValidateRefreshTokenAsync(string token, Guid userId);
    Task RevokeRefreshTokenAsync(string token);
}