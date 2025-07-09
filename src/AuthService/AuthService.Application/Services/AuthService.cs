using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IPasswordService passwordService,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
    {
        _logger.LogInformation("Login attempt for username: {Username}", loginDto.Username);

        // Get user by username
        var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
        if (user == null)
        {
            _logger.LogWarning("User not found: {Username}", loginDto.Username);
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        // Verify password
        if (!_passwordService.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid password for user: {Username}", loginDto.Username);
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            _logger.LogWarning("Inactive user login attempt: {Username}", loginDto.Username);
            throw new UnauthorizedAccessException("Account is inactive");
        }

        // Generate tokens
        var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user);
        var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync(user);

        // Save refresh token
        await _refreshTokenRepository.CreateAsync(refreshToken);

        _logger.LogInformation("Successful login for user: {Username}", loginDto.Username);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = user.GetRoles().Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description ?? string.Empty
                }).ToList()
            }
        };
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
    {
        _logger.LogInformation("Refresh token attempt");

        // Get refresh token from database
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (storedToken == null)
        {
            _logger.LogWarning("Refresh token not found");
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Check if token is expired
        if (storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token expired");
            await _refreshTokenRepository.DeleteAsync(storedToken.Id);
            throw new UnauthorizedAccessException("Refresh token expired");
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(storedToken.UserId);
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("User not found or inactive for refresh token");
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Generate new tokens
        var newAccessToken = await _jwtTokenService.GenerateAccessTokenAsync(user);
        var newRefreshToken = await _jwtTokenService.GenerateRefreshTokenAsync(user);

        // Replace old refresh token with new one
        await _refreshTokenRepository.DeleteAsync(storedToken.Id);
        await _refreshTokenRepository.CreateAsync(newRefreshToken);

        _logger.LogInformation("Successful token refresh for user: {UserId}", user.Id);

        return new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = newRefreshToken.ExpiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = user.GetRoles().Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description ?? string.Empty
                }).ToList()
            }
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        _logger.LogInformation("Logout attempt");

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (storedToken != null)
        {
            await _refreshTokenRepository.DeleteAsync(storedToken.Id);
            _logger.LogInformation("Refresh token invalidated for user: {UserId}", storedToken.UserId);
        }
    }

    public async Task LogoutAllAsync(Guid userId)
    {
        _logger.LogInformation("Logout all sessions for user: {UserId}", userId);

        var userTokens = await _refreshTokenRepository.GetByUserIdAsync(userId);
        foreach (var token in userTokens)
        {
            await _refreshTokenRepository.DeleteAsync(token.Id);
        }

        _logger.LogInformation("All refresh tokens invalidated for user: {UserId}", userId);
    }

    public async Task<bool> RequestPasswordResetAsync(ResetPasswordDto resetPasswordDto)
    {
        _logger.LogInformation("Password reset request for email: {Email}", resetPasswordDto.Email);

        var user = await _userRepository.GetByEmailAsync(resetPasswordDto.Email);
        if (user == null)
        {
            // Don't reveal if email exists for security
            _logger.LogInformation("Password reset requested for non-existent email: {Email}", resetPasswordDto.Email);
            return true;
        }

        // Generate a secure reset token
        var resetToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"); // 64 character token
        
        var passwordResetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = resetToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1) // Token expires in 1 hour
        };

        await _passwordResetTokenRepository.CreateAsync(passwordResetToken);
        
        // In a real application, you would send an email here
        _logger.LogInformation("Password reset token generated for user: {UserId}, Token: {Token}", user.Id, resetToken);
        
        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordConfirmDto resetPasswordConfirmDto)
    {
        _logger.LogInformation("Password reset confirmation for email: {Email}", resetPasswordConfirmDto.Email);

        // Get the reset token
        var resetToken = await _passwordResetTokenRepository.GetByTokenAsync(resetPasswordConfirmDto.Token);
        if (resetToken == null)
        {
            _logger.LogWarning("Invalid reset token attempted: {Token}", resetPasswordConfirmDto.Token);
            return false;
        }

        // Check if token is expired
        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Expired reset token attempted: {Token}", resetPasswordConfirmDto.Token);
            return false;
        }

        // Check if token has already been used
        if (resetToken.IsUsed)
        {
            _logger.LogWarning("Already used reset token attempted: {Token}", resetPasswordConfirmDto.Token);
            return false;
        }

        // Get the user and verify email matches
        var user = await _userRepository.GetByEmailAsync(resetPasswordConfirmDto.Email);
        if (user == null || user.Id != resetToken.UserId)
        {
            _logger.LogWarning("Reset token user mismatch. Email: {Email}, TokenUserId: {TokenUserId}", 
                resetPasswordConfirmDto.Email, resetToken.UserId);
            return false;
        }

        // Hash the new password
        var newPasswordHash = _passwordService.HashPassword(resetPasswordConfirmDto.NewPassword);
        
        // Update user password
        user.PasswordHash = newPasswordHash;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Mark token as used
        resetToken.IsUsed = true;
        resetToken.UsedAt = DateTime.UtcNow;
        await _passwordResetTokenRepository.UpdateAsync(resetToken);

        _logger.LogInformation("Password reset completed for user: {UserId}", user.Id);
        
        return true;
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var principal = await _jwtTokenService.ValidateTokenAsync(token);
            return principal != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<UserDto?> GetCurrentUserAsync(string token)
    {
        try
        {
            var principal = await _jwtTokenService.ValidateTokenAsync(token);
            if (principal == null)
            {
                return null;
            }

            var userIdClaim = principal.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return null;
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = user.GetRoles().Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description ?? string.Empty
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user from token");
            return null;
        }
    }
}
