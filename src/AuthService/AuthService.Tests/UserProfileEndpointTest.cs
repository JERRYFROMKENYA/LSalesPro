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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Tests;

public static class UserProfileEndpointTest
{
    public static async Task<bool> RunTestAsync()
    {
        try
        {
            Console.WriteLine("🔧 Setting up user profile test environment...");
            
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
                options.UseInMemoryDatabase("UserProfileTestDb"));
            
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
            
            // Setup test data and get token
            Console.WriteLine("🔧 Setting up test data and generating token...");
            var token = await SetupTestDataAndGenerateTokenAsync(serviceProvider);
            
            // Test 1: Valid token returns user profile
            Console.WriteLine("\n🧪 Test 1: Valid JWT token returns user profile");
            var test1Result = await TestValidTokenReturnsUserProfile(serviceProvider, token);
            
            // Test 2: Invalid token returns 401
            Console.WriteLine("\n🧪 Test 2: Invalid JWT token returns 401 Unauthorized");
            var test2Result = await TestInvalidTokenReturns401(serviceProvider);
            
            // Test 3: Missing Authorization header returns 401
            Console.WriteLine("\n🧪 Test 3: Missing Authorization header returns 401 Unauthorized");
            var test3Result = await TestMissingAuthHeaderReturns401(serviceProvider);
            
            // Test 4: Malformed Authorization header returns 401
            Console.WriteLine("\n🧪 Test 4: Malformed Authorization header returns 401 Unauthorized");
            var test4Result = await TestMalformedAuthHeaderReturns401(serviceProvider);
            
            var allTestsPassed = test1Result && test2Result && test3Result && test4Result;
            
            Console.WriteLine($"\n📊 Test Results Summary:");
            Console.WriteLine($"   Valid Token → User Profile: {(test1Result ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"   Invalid Token → 401: {(test2Result ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"   Missing Auth Header → 401: {(test3Result ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"   Malformed Auth Header → 401: {(test4Result ? "✅ PASS" : "❌ FAIL")}");
            
            return allTestsPassed;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Test setup failed: {ex.Message}");
            return false;
        }
    }

    private static async Task<string> SetupTestDataAndGenerateTokenAsync(ServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        
        // Ensure database is created
        await dbContext.Database.EnsureCreatedAsync();
        
        // Create test role
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Sales Manager",
            Description = "Test role for user profile test",
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
            Username = "profiletestuser",
            Email = "profiletest@example.com",
            FirstName = "Profile",
            LastName = "TestUser",
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
        
        // Generate JWT token for the user
        var token = await jwtTokenService.GenerateAccessTokenAsync(user);
        
        Console.WriteLine("✅ Test data setup and token generation completed");
        return token;
    }

    private static async Task<bool> TestValidTokenReturnsUserProfile(ServiceProvider serviceProvider, string token)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthController>>();
            
            var controller = new AuthController(authService, userService, logger);
            
            // Create a mock HTTP context with Authorization header
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Authorization = $"Bearer {token}";
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
            
            var result = await controller.GetCurrentUser();
            
            // Check if result is OkObjectResult (200 OK)
            if (result is Microsoft.AspNetCore.Mvc.OkObjectResult okResult)
            {
                var userProfile = okResult.Value as UserDto;
                if (userProfile != null && userProfile.Username == "profiletestuser")
                {
                    Console.WriteLine($"   ✅ Valid token successfully returned user profile");
                    Console.WriteLine($"   📝 User ID: {userProfile.Id}");
                    Console.WriteLine($"   📝 Username: {userProfile.Username}");
                    Console.WriteLine($"   📝 Email: {userProfile.Email}");
                    Console.WriteLine($"   📝 Full Name: {userProfile.FirstName} {userProfile.LastName}");
                    Console.WriteLine($"   📝 Roles: {string.Join(", ", userProfile.Roles.Select(r => r.Name))}");
                    return true;
                }
            }
            
            Console.WriteLine($"   ❌ Valid token test failed - unexpected result type or user data");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Valid token test failed: {ex.Message}");
            return false;
        }
    }

    private static async Task<bool> TestInvalidTokenReturns401(ServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthController>>();
            
            var controller = new AuthController(authService, userService, logger);
            
            // Create a mock HTTP context with invalid Authorization header
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Authorization = "Bearer invalid-token-12345";
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
            
            var result = await controller.GetCurrentUser();
            
            // Check if result is UnauthorizedObjectResult (401 Unauthorized)
            if (result is Microsoft.AspNetCore.Mvc.UnauthorizedObjectResult)
            {
                Console.WriteLine($"   ✅ Invalid token correctly returned 401 Unauthorized");
                return true;
            }
            
            Console.WriteLine($"   ❌ Invalid token test failed - expected 401 Unauthorized");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Invalid token test failed: {ex.Message}");
            return false;
        }
    }

    private static async Task<bool> TestMissingAuthHeaderReturns401(ServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthController>>();
            
            var controller = new AuthController(authService, userService, logger);
            
            // Create a mock HTTP context without Authorization header
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
            
            var result = await controller.GetCurrentUser();
            
            // Check if result is UnauthorizedObjectResult (401 Unauthorized)
            if (result is Microsoft.AspNetCore.Mvc.UnauthorizedObjectResult)
            {
                Console.WriteLine($"   ✅ Missing Authorization header correctly returned 401 Unauthorized");
                return true;
            }
            
            Console.WriteLine($"   ❌ Missing auth header test failed - expected 401 Unauthorized");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Missing auth header test failed: {ex.Message}");
            return false;
        }
    }

    private static async Task<bool> TestMalformedAuthHeaderReturns401(ServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthController>>();
            
            var controller = new AuthController(authService, userService, logger);
            
            // Create a mock HTTP context with malformed Authorization header
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Authorization = "InvalidFormat some-token";
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
            
            var result = await controller.GetCurrentUser();
            
            // Check if result is UnauthorizedObjectResult (401 Unauthorized)
            if (result is Microsoft.AspNetCore.Mvc.UnauthorizedObjectResult)
            {
                Console.WriteLine($"   ✅ Malformed Authorization header correctly returned 401 Unauthorized");
                return true;
            }
            
            Console.WriteLine($"   ❌ Malformed auth header test failed - expected 401 Unauthorized");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Malformed auth header test failed: {ex.Message}");
            return false;
        }
    }
}
