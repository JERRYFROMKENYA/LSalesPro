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

public class LoginEndpointTest : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AuthDbContext _context;

    public LoginEndpointTest()
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
            options.UseInMemoryDatabase("LoginTestDb"));
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
        SetupTestDataAsync().Wait();
    }

    private async Task SetupTestDataAsync()
    {
        await _context.Database.EnsureCreatedAsync();
        var passwordService = _serviceProvider.GetRequiredService<IPasswordService>();
        var role = new Role { Id = Guid.NewGuid(), Name = "Sales Manager", Description = "Test role", IsActive = true };
        _context.Roles.Add(role);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = passwordService.HashPassword("password123"),
            IsActive = true
        };
        _context.Users.Add(user);
        _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthController>>();
        var controller = new AuthController(authService, userService, logger);
        var loginDto = new LoginDto { Username = "testuser", Password = "password123" };

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LoginResponseDto>(okResult.Value);
        Assert.False(string.IsNullOrEmpty(response.AccessToken));
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthController>>();
        var controller = new AuthController(authService, userService, logger);
        var loginDto = new LoginDto { Username = "testuser", Password = "wrongpassword" };

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_WithMalformedRequest_ReturnsBadRequest()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthController>>();
        var controller = new AuthController(authService, userService, logger);
        var loginDto = new LoginDto { Username = "", Password = "password123" };
        controller.ModelState.AddModelError("Username", "Username is required");

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _serviceProvider.Dispose();
    }
}

