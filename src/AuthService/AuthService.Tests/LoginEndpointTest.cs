using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Services;
using AuthService.Application.Services;
using AuthService.Application.Interfaces;
using AuthService.Application.DTOs;
using AuthService.Domain.Entities;
using AuthService.Api.Controllers;

namespace AuthService.Tests;

public static class LoginEndpointTest
{
    public static async Task<bool> RunTestAsync()
    {
        try
        {
            Console.WriteLine("🔧 Setting up test environment...");
            
            // Setup configuration
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

            // Setup services
            var services = new ServiceCollection();
            
            // Add logging
            services.AddLogging(builder => builder.AddConsole());
            
            // Add DbContext with in-memory database
            services.AddDbContext<AuthDbContext>(options =>
                options.UseInMemoryDatabase("LoginTestDb"));
            
            // Add repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            
            // Add services
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IAuthService, Application.Services.AuthService>();
            services.AddScoped<IUserService, UserService>();
            
            // Add configuration
            services.AddSingleton<IConfiguration>(configuration);
            
            var serviceProvider = services.BuildServiceProvider();
            
            // Setup test data
            Console.WriteLine("🔧 Setting up test data...");
            await SetupTestDataAsync(serviceProvider);
            
            // Test 1: Valid login
            Console.WriteLine("\n🧪 Test 1: Valid login credentials");
            var test1Result = await TestValidLogin(serviceProvider);
            
            // Test 2: Invalid credentials  
            Console.WriteLine("\n🧪 Test 2: Invalid login credentials");
            var test2Result = await TestInvalidLogin(serviceProvider);
            
            // Test 3: Malformed request
            Console.WriteLine("\n🧪 Test 3: Malformed login request");
            var test3Result = await TestMalformedRequest(serviceProvider);
            
            var allTestsPassed = test1Result && test2Result && test3Result;
            
            Console.WriteLine($"\n📊 Test Results Summary:");
            Console.WriteLine($"   Valid Login: {(test1Result ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"   Invalid Login: {(test2Result ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"   Malformed Request: {(test3Result ? "✅ PASS" : "❌ FAIL")}");
            
            return allTestsPassed;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Test setup failed: {ex.Message}");
            return false;
        }
    }

    private static async Task SetupTestDataAsync(ServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
        
        // Ensure database is created
        await dbContext.Database.EnsureCreatedAsync();
        
        // Create test role
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Sales Manager",
            Description = "Test role for login test",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync();
        
        // Create test user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = passwordService.HashPassword("password123"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        // Assign role to user
        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            AssignedAt = DateTime.UtcNow
        };
        
        dbContext.UserRoles.Add(userRole);
        await dbContext.SaveChangesAsync();
        
        Console.WriteLine("✅ Test data setup completed");
    }

    private static async Task<bool> TestValidLogin(ServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthController>>();
            
            var controller = new AuthController(authService, userService, logger);
            
            var loginDto = new LoginDto
            {
                Username = "testuser",
                Password = "password123"
            };
            
            var result = await controller.Login(loginDto);
            
            // Check if result is OkObjectResult (200 OK)
            if (result is Microsoft.AspNetCore.Mvc.OkObjectResult okResult)
            {
                var response = okResult.Value as LoginResponseDto;
                if (response != null && !string.IsNullOrEmpty(response.AccessToken))
                {
                    Console.WriteLine($"   ✅ Valid login successful");
                    Console.WriteLine($"   📝 Access token received (length: {response.AccessToken.Length})");
                    Console.WriteLine($"   📝 Refresh token received: {!string.IsNullOrEmpty(response.RefreshToken)}");
                    return true;
                }
            }
            
            Console.WriteLine($"   ❌ Valid login failed - unexpected result type");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Valid login test failed: {ex.Message}");
            return false;
        }
    }

    private static async Task<bool> TestInvalidLogin(ServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthController>>();
            
            var controller = new AuthController(authService, userService, logger);
            
            var loginDto = new LoginDto
            {
                Username = "testuser",
                Password = "wrongpassword"
            };
            
            var result = await controller.Login(loginDto);
            
            // Check if result is UnauthorizedObjectResult (401 Unauthorized)
            if (result is Microsoft.AspNetCore.Mvc.UnauthorizedObjectResult)
            {
                Console.WriteLine($"   ✅ Invalid login correctly rejected with 401 Unauthorized");
                return true;
            }
            
            Console.WriteLine($"   ❌ Invalid login test failed - expected 401 Unauthorized");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Invalid login test failed: {ex.Message}");
            return false;
        }
    }

    private static async Task<bool> TestMalformedRequest(ServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthController>>();
            
            var controller = new AuthController(authService, userService, logger);
            
            // Test with empty username
            var loginDto = new LoginDto
            {
                Username = "",
                Password = "password123"
            };
            
            // Simulate ModelState validation error
            controller.ModelState.AddModelError("Username", "Username is required");
            
            var result = await controller.Login(loginDto);
            
            // Check if result is BadRequestObjectResult (400 Bad Request)
            if (result is Microsoft.AspNetCore.Mvc.BadRequestObjectResult)
            {
                Console.WriteLine($"   ✅ Malformed request correctly rejected with 400 Bad Request");
                return true;
            }
            
            Console.WriteLine($"   ❌ Malformed request test failed - expected 400 Bad Request");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Malformed request test failed: {ex.Message}");
            return false;
        }
    }
}
