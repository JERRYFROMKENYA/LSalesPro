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

public static class RefreshTokenEndpointTest
{
    public static async Task<bool> RunTestAsync()
    {
        try
        {
            Console.WriteLine("🔧 Setting up refresh token test environment...");
            
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
                options.UseInMemoryDatabase("RefreshTokenTestDb"));
            
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
            var validRefreshToken = await SetupTestDataAsync(serviceProvider);
            
            // Test 1: Valid refresh token
            Console.WriteLine("\n🧪 Test 1: Valid refresh token");
            var test1Result = await TestValidRefreshToken(serviceProvider, validRefreshToken);
            
            // Test 2: Invalid refresh token  
            Console.WriteLine("\n🧪 Test 2: Invalid refresh token");
            var test2Result = await TestInvalidRefreshToken(serviceProvider);
            
            // Test 3: Expired refresh token
            Console.WriteLine("\n🧪 Test 3: Expired refresh token");
            var test3Result = await TestExpiredRefreshToken(serviceProvider);
            
            // Test 4: Malformed request
            Console.WriteLine("\n🧪 Test 4: Malformed refresh token request");
            var test4Result = await TestMalformedRefreshRequest(serviceProvider);
            
            var allTestsPassed = test1Result && test2Result && test3Result && test4Result;
            
            Console.WriteLine($"\n📊 Refresh Token Test Results Summary:");
            Console.WriteLine($"   Valid Refresh Token: {(test1Result ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"   Invalid Refresh Token: {(test2Result ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"   Expired Refresh Token: {(test3Result ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"   Malformed Request: {(test4Result ? "✅ PASS" : "❌ FAIL")}");
            
            return allTestsPassed;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Refresh token test setup failed: {ex.Message}");
            return false;
        }
    }

    private static async Task<string> SetupTestDataAsync(ServiceProvider serviceProvider)
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
            Description = "Test role for refresh token test",
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
            Username = "refreshtestuser",
            Email = "refreshtest@example.com",
            FirstName = "Refresh",
            LastName = "Test",
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
        
        // Generate a valid refresh token
        var refreshTokenEntity = await jwtTokenService.GenerateRefreshTokenAsync(user);
        dbContext.RefreshTokens.Add(refreshTokenEntity);
        await dbContext.SaveChangesAsync();
        
        Console.WriteLine("✅ Refresh token test data setup completed");
        return refreshTokenEntity.Token;
    }

    private static async Task<bool> TestValidRefreshToken(ServiceProvider serviceProvider, string validRefreshToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthController>>();
            
            var controller = new AuthController(authService, userService, logger);
            
            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = validRefreshToken
            };
            
            var result = await controller.RefreshToken(refreshTokenDto);
            
            // Check if result is OkObjectResult (200 OK)
            if (result is Microsoft.AspNetCore.Mvc.OkObjectResult okResult)
            {
                var response = okResult.Value as LoginResponseDto;
                if (response != null && !string.IsNullOrEmpty(response.AccessToken))
                {
                    Console.WriteLine($"   ✅ Valid refresh token successful");
                    Console.WriteLine($"   📝 New access token received (length: {response.AccessToken.Length})");
                    Console.WriteLine($"   📝 New refresh token received: {!string.IsNullOrEmpty(response.RefreshToken)}");
                    Console.WriteLine($"   📝 Token expires at: {response.ExpiresAt}");
                    return true;
                }
            }
            
            Console.WriteLine($"   ❌ Valid refresh token failed - unexpected result type");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Valid refresh token test failed: {ex.Message}");
            return false;
        }
    }

    private static async Task<bool> TestInvalidRefreshToken(ServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthController>>();
            
            var controller = new AuthController(authService, userService, logger);
            
            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = "invalid-refresh-token-that-does-not-exist"
            };
            
            var result = await controller.RefreshToken(refreshTokenDto);
            
            // Check if result is UnauthorizedObjectResult (401 Unauthorized)
            if (result is Microsoft.AspNetCore.Mvc.UnauthorizedObjectResult)
            {
                Console.WriteLine($"   ✅ Invalid refresh token correctly rejected with 401 Unauthorized");
                return true;
            }
            
            Console.WriteLine($"   ❌ Invalid refresh token test failed - expected 401 Unauthorized");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Invalid refresh token test failed: {ex.Message}");
            return false;
        }
    }

    private static async Task<bool> TestExpiredRefreshToken(ServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthController>>();
            
            var controller = new AuthController(authService, userService, logger);
            
            // Create an expired refresh token
            var expiredToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = Guid.NewGuid().ToString(),
                UserId = (await dbContext.Users.FirstAsync()).Id,
                ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired yesterday
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };
            
            dbContext.RefreshTokens.Add(expiredToken);
            await dbContext.SaveChangesAsync();
            
            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = expiredToken.Token
            };
            
            var result = await controller.RefreshToken(refreshTokenDto);
            
            // Check if result is UnauthorizedObjectResult (401 Unauthorized)
            if (result is Microsoft.AspNetCore.Mvc.UnauthorizedObjectResult)
            {
                Console.WriteLine($"   ✅ Expired refresh token correctly rejected with 401 Unauthorized");
                return true;
            }
            
            Console.WriteLine($"   ❌ Expired refresh token test failed - expected 401 Unauthorized");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Expired refresh token test failed: {ex.Message}");
            return false;
        }
    }

    private static async Task<bool> TestMalformedRefreshRequest(ServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthController>>();
            
            var controller = new AuthController(authService, userService, logger);
            
            // Test with empty refresh token
            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = ""
            };
            
            // Simulate ModelState validation error
            controller.ModelState.AddModelError("RefreshToken", "RefreshToken is required");
            
            var result = await controller.RefreshToken(refreshTokenDto);
            
            // Check if result is BadRequestObjectResult (400 Bad Request)
            if (result is Microsoft.AspNetCore.Mvc.BadRequestObjectResult)
            {
                Console.WriteLine($"   ✅ Malformed refresh request correctly rejected with 400 Bad Request");
                return true;
            }
            
            Console.WriteLine($"   ❌ Malformed refresh request test failed - expected 400 Bad Request");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Malformed refresh request test failed: {ex.Message}");
            return false;
        }
    }
}
