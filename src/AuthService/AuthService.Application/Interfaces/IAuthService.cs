using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
    Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task LogoutAllAsync(Guid userId);
    Task<bool> RequestPasswordResetAsync(ResetPasswordDto resetPasswordDto);
    Task<bool> ResetPasswordAsync(ResetPasswordConfirmDto resetPasswordConfirmDto);
    Task<bool> ValidateTokenAsync(string token);
    Task<UserDto?> GetCurrentUserAsync(string token);
}
