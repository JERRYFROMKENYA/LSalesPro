using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Exceptions;
// using AuthService.Infrastructure.Repositories;
using System;
using System.Threading.Tasks;

namespace AuthService.Application.Services;

public class PasswordResetService : IPasswordResetService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IPasswordService _passwordService;

    public PasswordResetService(
        IUserRepository userRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IPasswordService passwordService)
    {
        _userRepository = userRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _passwordService = passwordService;
    }

    public async Task RequestPasswordResetAsync(ForgotPasswordDto forgotPasswordDto)
    {
        var user = await _userRepository.GetByEmailAsync(forgotPasswordDto.Email);
        if (user == null)
        {
            // Do not reveal if the email exists for security reasons
            return;
        }

        var resetToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var passwordResetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = resetToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        await _passwordResetTokenRepository.CreateAsync(passwordResetToken);

        // Send email with reset token (email service integration required)
    }

    public async Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        var resetToken = await _passwordResetTokenRepository.GetByTokenAsync(resetPasswordDto.Token);
        if (resetToken == null || resetToken.ExpiresAt < DateTime.UtcNow || resetToken.IsUsed)
        {
            return new ResetPasswordResponseDto
            {
                Success = false,
                Message = "Invalid or expired reset token."
            };
        }

        var user = await _userRepository.GetByIdAsync(resetToken.UserId);
        if (user == null)
        {
            return new ResetPasswordResponseDto
            {
                Success = false,
                Message = "User not found."
            };
        }

        user.PasswordHash = _passwordService.HashPassword(resetPasswordDto.NewPassword);
        await _userRepository.UpdateAsync(user);

        resetToken.IsUsed = true;
        resetToken.UsedAt = DateTime.UtcNow;
        await _passwordResetTokenRepository.UpdateAsync(resetToken);

        return new ResetPasswordResponseDto
        {
            Success = true,
            Message = "Password reset successfully."
        };
    }
}
