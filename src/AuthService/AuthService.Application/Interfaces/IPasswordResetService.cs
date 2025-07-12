using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces;

public interface IPasswordResetService
{
    Task RequestPasswordResetAsync(ForgotPasswordDto forgotPasswordDto);
    Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
}
