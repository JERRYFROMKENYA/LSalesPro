using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AuthService.Api.Services;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Services;
using Shared.Contracts.Auth;
using Xunit;
using PasswordService = AuthService.Application.Services.PasswordService;

namespace AuthService.Tests;

public class AuthGrpcServiceTest : IDisposable
{
    private readonly IServiceScope _scope;
    private readonly AuthDbContext _context;
    private readonly IUserService _userService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly AuthGrpcService _grpcService;

    public AuthGrpcServiceTest()
    {
        var services = new ServiceCollection();
        
        services.AddDbContext<AuthDbContext>(options =>
            options.UseInMemoryDatabase($"AuthServiceDb_Test_{Guid.NewGuid()}"));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService.Application.Services.AuthService>();

        services.AddLogging(builder => builder.AddConsole());

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "your-super-secret-key-that-is-at-least-32-characters-long!",
                ["Jwt:Issuer"] = "LSalesPro.AuthService",
                ["Jwt:Audience"] = "LSalesPro.Services",
                ["Jwt:AccessTokenExpirationMinutes"] = "60",
                ["Jwt:RefreshTokenExpirationDays"] = "7"
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        var serviceProvider = services.BuildServiceProvider();
        _scope = serviceProvider.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        _userService = _scope.ServiceProvider.GetRequiredService<IUserService>();
        _jwtTokenService = _scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        _grpcService = new AuthGrpcService(
            _scope.ServiceProvider.GetRequiredService<IAuthService>(),
            _userService,
            _jwtTokenService,
            _scope.ServiceProvider.GetRequiredService<ILogger<AuthGrpcService>>());
    }

    [Fact]
    public async Task ValidateToken_ShouldReturnValidResponse_WhenTokenIsValid()
    {
        // Arrange
        var validToken = await _jwtTokenService.GenerateAccessTokenAsync(new User
        {
            Id = Guid.NewGuid(),
            Username = "test-user",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        });

        var request = new ValidateTokenRequest { Token = validToken };

        // Act
        var response = await _grpcService.ValidateToken(request, null);

        // Assert
        Assert.True(response.IsValid);
        Assert.Equal("test-user-id", response.UserId);
    }

    [Fact]
    public async Task GetUserPermissions_ShouldReturnPermissions_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _context.Users.Add(new User
        {
            Id = userId,
            Username = "test-user",
            Email = "test@example.com",
            UserRoles = new List<UserRole>
            {
                new UserRole { Role = new Role { Name = "Sales Manager", RolePermissions = new List<RolePermission> { new RolePermission { Permission = new Permission { Name = "test-permission" } } } } }
            }
        });
        await _context.SaveChangesAsync();

        var request = new GetUserPermissionsRequest { UserId = userId.ToString() };

        // Act
        var response = await _grpcService.GetUserPermissions(request, null);

        // Assert
        Assert.Contains("test-permission", response.Permissions);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnUserDetails_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _context.Users.Add(new User
        {
            Id = userId,
            Username = "test-user",
            Email = "test@example.com",
            UserRoles = new List<UserRole>
            {
                new UserRole { Role = new Role { Name = "Sales Manager", RolePermissions = new List<RolePermission> { new RolePermission { Permission = new Permission { Name = "test-permission" } } } } }
            }
        });
        await _context.SaveChangesAsync();

        var request = new GetUserByIdRequest { UserId = userId.ToString() };

        // Act
        var response = await _grpcService.GetUserById(request, null);

        // Assert
        Assert.Equal(userId.ToString(), response.User.Id);
        Assert.Equal("test-user", response.User.Username);
        Assert.Equal("test@example.com", response.User.Email);
        Assert.Contains("Sales Manager", response.User.Roles);
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
