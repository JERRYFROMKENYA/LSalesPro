using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using AuthService.Application.Services;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Xunit;

namespace AuthService.Tests;

public class JwtTokenServiceTest : IDisposable
{
    private readonly AuthDbContext _context;
    private readonly JwtTokenService _jwtTokenService;
    private readonly User _testUser;

    public JwtTokenServiceTest()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "your-super-secret-key-that-is-at-least-32-characters-long-for-testing!",
            ["Jwt:Issuer"] = "LSalesPro.AuthService.Test",
            ["Jwt:Audience"] = "LSalesPro.Services.Test",
            ["Jwt:AccessTokenExpirationMinutes"] = "30",
            ["Jwt:RefreshTokenExpirationDays"] = "7"
        });
        var configuration = configBuilder.Build();

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AuthDbContext(options);
        _context.Database.EnsureCreated();

        _jwtTokenService = new JwtTokenService(configuration);

        _testUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "jwttest001",
            Email = "jwttest@example.com",
            PasswordHash = "hashedpassword123",
            FirstName = "JWT",
            LastName = "TestUser",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(_testUser);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GenerateAccessTokenAsync_WithValidUser_ReturnsValidToken()
    {
        // Act
        var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(_testUser);

        // Assert
        Assert.False(string.IsNullOrEmpty(accessToken));
    }

    [Fact]
    public async Task ValidateTokenAsync_WithValidToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(_testUser);

        // Act
        var claimsPrincipal = await _jwtTokenService.ValidateTokenAsync(accessToken);

        // Assert
        Assert.NotNull(claimsPrincipal);
        Assert.Equal(_testUser.Id.ToString(), claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal(_testUser.Username, claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value);
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_WithValidUser_ReturnsValidRefreshToken()
    {
        // Act
        var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync(_testUser);

        // Assert
        Assert.NotNull(refreshToken);
        Assert.False(string.IsNullOrEmpty(refreshToken.Token));
        Assert.Equal(_testUser.Id, refreshToken.UserId);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync(_testUser);
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        var isValid = await _jwtTokenService.ValidateRefreshTokenAsync(refreshToken.Token, _testUser.Id);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithInvalidToken_ReturnsNull()
    {
        // Act
        var claimsPrincipal = await _jwtTokenService.ValidateTokenAsync("invalid.token.here");

        // Assert
        Assert.Null(claimsPrincipal);
    }

    [Fact]
    public async Task GenerateAccessTokenAsync_ShouldIncludeClaims_WhenUserIsValid()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "testuser@example.com",
            UserRoles = new List<UserRole>
            {
                new UserRole { Role = new Role { Name = "Sales Manager" } }
            }
        };

        // Act
        var token = await _jwtTokenService.GenerateAccessTokenAsync(user);
        var claimsPrincipal = await _jwtTokenService.ValidateTokenAsync(token);

        // Assert
        Assert.NotNull(claimsPrincipal);
        Assert.Equal(user.Id.ToString(), claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal(user.Username, claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal(user.Email, claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value);
        Assert.Contains("Sales Manager", claimsPrincipal.FindFirst("role")?.Value);
    }

    [Fact]
    public async Task ValidateTokenAsync_ShouldReturnNull_WhenTokenIsInvalid()
    {
        // Act
        var claimsPrincipal = await _jwtTokenService.ValidateTokenAsync("invalid-token");

        // Assert
        Assert.Null(claimsPrincipal);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
