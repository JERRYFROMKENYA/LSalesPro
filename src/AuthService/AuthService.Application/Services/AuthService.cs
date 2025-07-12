using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace AuthService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;
    private readonly IMemoryCache _memoryCache;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IPasswordService passwordService,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger,
        IMemoryCache memoryCache)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _memoryCache = memoryCache;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
    {
        _logger.LogInformation("Login attempt for username: {Username}", loginDto.Username);

        // Get user by username
        var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
        if (user == null)
        {
            _logger.LogWarning("User not found: {Username}", loginDto.Username);
            throw new UserNotFoundException(loginDto.Username);
            
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

        var userDto = new UserDto
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

        // Cache user data
        _memoryCache.Set(user.Id, userDto, TimeSpan.FromMinutes(30)); // Cache for 30 minutes

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt,
            User = userDto
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

        var userDto = new UserDto
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

        // Update cached user data
        _memoryCache.Set(user.Id, userDto, TimeSpan.FromMinutes(30)); // Cache for 30 minutes

        return new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = newRefreshToken.ExpiresAt,
            User = userDto
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

            // Invalidate user cache
            _memoryCache.Remove(storedToken.UserId);
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

        // Invalidate user cache
        _memoryCache.Remove(userId);
    }

    public async Task<bool> RequestPasswordResetAsync(ResetPasswordDto resetPasswordDto)
    {
        _logger.LogInformation("Password reset request for email: {Email}", resetPasswordDto.Email);

        var user = await _userRepository.GetByEmailAsync(resetPasswordDto.Email);
        if (user == null)
        {
            // Don't reveal if email exists for security
            _logger.LogInformation("Password reset requested for non-existent email: {Email}", resetPasswordDto.Email);
            return false; // Changed to false to indicate failure
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
        
        // Send email with reset token (email service integration required)
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

        // Invalidate user cache
        _memoryCache.Remove(user.Id);

        _logger.LogInformation("Password reset completed for user: {UserId}", user.Id);
        
        return true;
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var isValid = _jwtTokenService.ValidateToken(token);
            return isValid;
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
            var isValid = _jwtTokenService.ValidateToken(token);
            if (!isValid)
            {
                return null;
            }

            // Since ValidateToken returns bool, we need to re-validate to get claims
            var principal = await _jwtTokenService.ValidateTokenAsync(token);
            if (principal == null)
            {
                return null;
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return null;
            }

            // Try to get user from cache
            if (_memoryCache.TryGetValue(userId, out UserDto? cachedUser))
            {
                _logger.LogInformation("User {UserId} found in cache.", userId);
                return cachedUser;
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var userDto = new UserDto
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

            // Cache user data
            _memoryCache.Set(userId, userDto, TimeSpan.FromMinutes(30)); // Cache for 30 minutes

            return userDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user from token");
            return null;
        }
    }
}
