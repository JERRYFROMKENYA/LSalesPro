using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AuthService.Api.Controllers;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Services;
using Xunit;
using PasswordService = AuthService.Application.Services.PasswordService;

namespace AuthService.Tests;

public class RefreshTokenEndpointTest : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AuthDbContext _context;
    private readonly string _validRefreshToken;

    public RefreshTokenEndpointTest()
    {
        var services = new ServiceCollection();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsAVeryLongSecretKeyForJWTTokenGeneration123456789",
                ["Jwt:Issuer"] = "LSalesProAuthService",
                ["Jwt:Audience"] = "LSalesProClients",
                ["Jwt:AccessTokenExpirationMinutes"] = "30",
                ["Jwt:RefreshTokenExpirationDays"] = "7"
            })
            .Build();

        services.AddLogging(builder => builder.AddConsole());
        services.AddDbContext<AuthDbContext>(options =>
            options.UseInMemoryDatabase("RefreshTokenTestDb"));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, Application.Services.AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<IConfiguration>(configuration);
        
        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<AuthDbContext>();
        _validRefreshToken = SetupTestDataAsync().Result;
    }

    private async Task<string> SetupTestDataAsync()
    {
        await _context.Database.EnsureCreatedAsync();
        var passwordService = _serviceProvider.GetRequiredService<IPasswordService>();
        var jwtTokenService = _serviceProvider.GetRequiredService<IJwtTokenService>();
        var role = new Role { Id = Guid.NewGuid(), Name = "Sales Manager", Description = "Test role", IsActive = true };
        _context.Roles.Add(role);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "refreshtestuser",
            Email = "refreshtest@example.com",
            FirstName = "Refresh",
            LastName = "Test",
            PasswordHash = passwordService.HashPassword("password123"),
            IsActive = true
        };
        _context.Users.Add(user);
        _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        await _context.SaveChangesAsync();
        var refreshTokenEntity = await jwtTokenService.GenerateRefreshTokenAsync(user);
        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();
        return refreshTokenEntity.Token;
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsOk()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthController>>();
        var controller = new AuthController(authService, userService, logger);
        var refreshTokenDto = new RefreshTokenDto { RefreshToken = _validRefreshToken };

        // Act
        var result = await controller.RefreshToken(refreshTokenDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LoginResponseDto>(okResult.Value);
        Assert.False(string.IsNullOrEmpty(response.AccessToken));
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthController>>();
        var controller = new AuthController(authService, userService, logger);
        var refreshTokenDto = new RefreshTokenDto { RefreshToken = "invalid-token" };

        // Act
        var result = await controller.RefreshToken(refreshTokenDto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task RefreshToken_WithExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthController>>();
        var controller = new AuthController(authService, userService, logger);
        var expiredToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = Guid.NewGuid().ToString(),
            UserId = (await _context.Users.FirstAsync()).Id,
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };
        _context.RefreshTokens.Add(expiredToken);
        await _context.SaveChangesAsync();
        var refreshTokenDto = new RefreshTokenDto { RefreshToken = expiredToken.Token };

        // Act
        var result = await controller.RefreshToken(refreshTokenDto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _serviceProvider.Dispose();
    }
}
