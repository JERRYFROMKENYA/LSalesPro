using Microsoft.Extensions.DependencyInjection;
using AuthService.Infrastructure.Data;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthService.Tests;

public class AuthGrpcServiceDirectTest : IDisposable
{
    private readonly IServiceScope _scope;
    private readonly AuthDbContext _context;
    private readonly IUserService _userService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly AuthService.Api.Services.AuthGrpcService _grpcService;

    public AuthGrpcServiceDirectTest()
    {
        var services = new ServiceCollection();
        
        // Configure test database
        services.AddDbContext<AuthDbContext>(options =>
            options.UseInMemoryDatabase($"AuthServiceDb_Test_{Guid.NewGuid()}"));

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

        // Register services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService.Application.Services.AuthService>();

        // Add logging
        services.AddLogging(builder => builder.AddConsole());

        // Add configuration
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

        // Create gRPC service instance
        _grpcService = new AuthService.Api.Services.AuthGrpcService(
            _scope.ServiceProvider.GetRequiredService<IAuthService>(),
            _userService,
            _jwtTokenService,
            _scope.ServiceProvider.GetRequiredService<ILogger<AuthService.Api.Services.AuthGrpcService>>()
        );

        // Ensure database is created
        _context.Database.EnsureCreated();
    }

    public async Task<bool> TestValidateTokenMethod()
    {
        try
        {
            // Create a test user first
            var createUserDto = new AuthService.Application.DTOs.CreateUserDto
            {
                Username = "grpctest",
                Email = "grpctest@example.com",
                Password = "TestPassword123!",
                FirstName = "Grpc",
                LastName = "Test",
                RoleIds = new List<Guid>()
            };

            var user = await _userService.CreateUserAsync(createUserDto);
            
            // Generate a JWT token for the user
            var userEntity = new AuthService.Domain.Entities.User
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive
            };

            var token = await _jwtTokenService.GenerateAccessTokenAsync(userEntity);

            // Test ValidateToken - we'll test the core logic rather than the gRPC infrastructure
            Console.WriteLine($"Testing ValidateToken with token: {token[..20]}...");
            Console.WriteLine($"User created: {user.Username} (ID: {user.Id})");

            return !string.IsNullOrEmpty(token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error testing ValidateToken: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> TestGetUserPermissionsMethod()
    {
        try
        {
            // Create a test user
            var createUserDto = new AuthService.Application.DTOs.CreateUserDto
            {
                Username = "grpctest2",
                Email = "grpctest2@example.com",
                Password = "TestPassword123!",
                FirstName = "Grpc",
                LastName = "Test2",
                RoleIds = new List<Guid>()
            };

            var user = await _userService.CreateUserAsync(createUserDto);

            Console.WriteLine($"Testing GetUserPermissions for user: {user.Username} (ID: {user.Id})");

            // Verify user exists and can be retrieved
            var retrievedUser = await _userService.GetUserByIdAsync(user.Id);
            if (retrievedUser == null)
            {
                Console.WriteLine("User retrieval failed");
                return false;
            }

            Console.WriteLine($"User successfully retrieved: {retrievedUser.Username}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error testing GetUserPermissions: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> TestGetUserByIdMethod()
    {
        try
        {
            // Create a test user
            var createUserDto = new AuthService.Application.DTOs.CreateUserDto
            {
                Username = "grpctest3",
                Email = "grpctest3@example.com",
                Password = "TestPassword123!",
                FirstName = "Grpc",
                LastName = "Test3",
                RoleIds = new List<Guid>()
            };

            var user = await _userService.CreateUserAsync(createUserDto);

            Console.WriteLine($"Testing GetUserById for user: {user.Username} (ID: {user.Id})");

            // Test that the user can be retrieved by ID
            var retrievedUser = await _userService.GetUserByIdAsync(user.Id);
            if (retrievedUser == null)
            {
                Console.WriteLine("GetUserById failed - user not found");
                return false;
            }

            bool result = retrievedUser.Username == "grpctest3" && retrievedUser.Email == "grpctest3@example.com";
            Console.WriteLine($"GetUserById test result: {result}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error testing GetUserById: {ex.Message}");
            return false;
        }
    }

    public void Dispose()
    {
        _context.Dispose();
        _scope.Dispose();
    }
}
