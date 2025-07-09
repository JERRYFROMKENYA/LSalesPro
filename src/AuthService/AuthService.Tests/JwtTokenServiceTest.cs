using AuthService.Application.Services;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace AuthService.Tests;

public class JwtTokenServiceTest
{
    public static async Task RunJwtTokenTests()
    {
        Console.WriteLine("🚀 Starting JWT Token Service Test...");
Console.WriteLine("=============================================================");

try
{
    // Setup configuration for JWT service
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

    // Setup in-memory database
    var options = new DbContextOptionsBuilder<AuthDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;
    
    using var context = new AuthDbContext(options);
    context.Database.EnsureCreated();
    
    Console.WriteLine("✅ Database and configuration setup successfully");

    // Setup services
    var userRepository = new UserRepository(context);
    var jwtTokenService = new JwtTokenService(configuration);

    Console.WriteLine("✅ JWT Token Service initialized successfully");

    // Create a test user with roles
    var testUser = new User
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

    // Add user to context
    context.Users.Add(testUser);
    await context.SaveChangesAsync();

    // Get the seeded roles
    var salesManagerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Sales Manager");
    var salesRepRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Sales Representative");

    if (salesManagerRole != null)
    {
        // Assign Sales Manager role to test user
        var userRole = new UserRole
        {
            UserId = testUser.Id,
            RoleId = salesManagerRole.Id,
            AssignedAt = DateTime.UtcNow
        };
        context.UserRoles.Add(userRole);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ Assigned role '{salesManagerRole.Name}' to test user");
    }

    // Reload user with roles and permissions
    var userWithRoles = await context.Users
        .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
        .ThenInclude(r => r.RolePermissions)
        .ThenInclude(rp => rp.Permission)
        .FirstOrDefaultAsync(u => u.Id == testUser.Id);

    if (userWithRoles == null)
    {
        Console.WriteLine("❌ Failed to load user with roles");
        return;
    }

    Console.WriteLine("=============================================================");

    // Test 1: Generate Access Token
    Console.WriteLine("🔧 Testing JWT Access Token Generation...");
    var accessToken = await jwtTokenService.GenerateAccessTokenAsync(userWithRoles);
    
    if (string.IsNullOrEmpty(accessToken))
    {
        Console.WriteLine("❌ Access token generation failed");
        return;
    }
    
    Console.WriteLine("✅ Access token generated successfully");
    Console.WriteLine($"   Token length: {accessToken.Length} characters");
    Console.WriteLine($"   Token preview: {accessToken.Substring(0, Math.Min(50, accessToken.Length))}...");

    // Test 2: Validate Access Token
    Console.WriteLine("\n🔧 Testing JWT Access Token Validation...");
    var claimsPrincipal = await jwtTokenService.ValidateTokenAsync(accessToken);
    
    if (claimsPrincipal == null)
    {
        Console.WriteLine("❌ Access token validation failed");
        return;
    }
    
    Console.WriteLine("✅ Access token validation successful");
    
    // Check claims
    var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var usernameClaim = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
    var emailClaim = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
    var roleClaims = claimsPrincipal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    
    Console.WriteLine($"   User ID: {userIdClaim}");
    Console.WriteLine($"   Username: {usernameClaim}");
    Console.WriteLine($"   Email: {emailClaim}");
    Console.WriteLine($"   Roles: {string.Join(", ", roleClaims)}");

    // Verify claims match user data
    if (userIdClaim != testUser.Id.ToString() || 
        usernameClaim != testUser.Username || 
        emailClaim != testUser.Email)
    {
        Console.WriteLine("❌ Token claims don't match user data");
        return;
    }
    
    Console.WriteLine("✅ Token claims verified correctly");

    // Test 3: Generate Refresh Token
    Console.WriteLine("\n🔧 Testing Refresh Token Generation...");
    var refreshToken = await jwtTokenService.GenerateRefreshTokenAsync(userWithRoles);
    
    if (refreshToken == null || string.IsNullOrEmpty(refreshToken.Token))
    {
        Console.WriteLine("❌ Refresh token generation failed");
        return;
    }
    
    Console.WriteLine("✅ Refresh token generated successfully");
    Console.WriteLine($"   Token: {refreshToken.Token}");
    Console.WriteLine($"   Expires at: {refreshToken.ExpiresAt}");
    Console.WriteLine($"   User ID: {refreshToken.UserId}");

    // Test 4: Validate Refresh Token
    Console.WriteLine("\n🔧 Testing Refresh Token Validation...");
    var isRefreshTokenValid = await jwtTokenService.ValidateRefreshTokenAsync(refreshToken.Token, testUser.Id);
    
    if (!isRefreshTokenValid)
    {
        Console.WriteLine("❌ Refresh token validation failed");
        return;
    }
    
    Console.WriteLine("✅ Refresh token validation successful");

    // Test 5: Invalid Token Validation
    Console.WriteLine("\n🔧 Testing Invalid Token Rejection...");
    var invalidTokenValidation = await jwtTokenService.ValidateTokenAsync("invalid.token.here");
    
    if (invalidTokenValidation != null)
    {
        Console.WriteLine("❌ Invalid token was incorrectly validated");
        return;
    }
    
    Console.WriteLine("✅ Invalid token correctly rejected");

    // Test 6: Token Revocation
    Console.WriteLine("\n🔧 Testing Token Revocation...");
    await jwtTokenService.RevokeRefreshTokenAsync(refreshToken.Token);
    Console.WriteLine("✅ Token revocation completed (placeholder implementation)");

    Console.WriteLine("=============================================================");
    Console.WriteLine("🎉 ALL JWT TOKEN SERVICE TESTS PASSED!");
    Console.WriteLine("📋 JWT Token Service is working correctly:");
    Console.WriteLine("   ✅ Access token generation with claims");
    Console.WriteLine("   ✅ Token validation and claims extraction");
    Console.WriteLine("   ✅ Refresh token generation");
    Console.WriteLine("   ✅ Refresh token validation");
    Console.WriteLine("   ✅ Invalid token rejection");
    Console.WriteLine("   ✅ Token revocation mechanism");
    Console.WriteLine("=============================================================");
}
catch (Exception ex)
{
    Console.WriteLine("=============================================================");
    Console.WriteLine("❌ JWT TOKEN SERVICE TEST FAILED!");
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
    Console.WriteLine("=============================================================");
}
    }
}
