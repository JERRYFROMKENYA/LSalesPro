using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using AuthService.Infrastructure.Data;
using AuthService.Domain.Entities;
using AuthService.Application.Interfaces;

namespace AuthService.Tests;

public static class PasswordResetEndpointTest
{
    public static async Task<bool> RunTestAsync()
    {
        Console.WriteLine("🔐 Testing Password Reset Endpoints...");
        
        try
        {
            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Testing");
                });

            var client = application.CreateClient();

            using var scope = application.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();

            // Ensure database is created and seeded
            await dbContext.Database.EnsureCreatedAsync();

            // Create a test user
            var testUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "resetuser",
                Email = "resetuser@example.com",
                PasswordHash = passwordService.HashPassword("OriginalPassword123!"),
                FirstName = "Reset",
                LastName = "User",
                IsActive = true
            };

            dbContext.Users.Add(testUser);
            await dbContext.SaveChangesAsync();

            // Test 1: Request password reset - Valid email
            Console.WriteLine("  ✅ Test 1: Request password reset with valid email");
            var resetRequest = new
            {
                email = testUser.Email
            };

            var resetRequestJson = JsonSerializer.Serialize(resetRequest);
            var resetRequestContent = new StringContent(resetRequestJson, Encoding.UTF8, "application/json");
            var resetRequestResponse = await client.PostAsync("/api/v1/auth/password/reset-request", resetRequestContent);

            if (!resetRequestResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Password reset request failed: {resetRequestResponse.StatusCode}");
                return false;
            }

            // Test 2: Request password reset - Invalid email (should still return success for security)
            Console.WriteLine("  ✅ Test 2: Request password reset with invalid email");
            var invalidResetRequest = new
            {
                email = "nonexistent@example.com"
            };

            var invalidResetRequestJson = JsonSerializer.Serialize(invalidResetRequest);
            var invalidResetRequestContent = new StringContent(invalidResetRequestJson, Encoding.UTF8, "application/json");
            var invalidResetRequestResponse = await client.PostAsync("/api/v1/auth/password/reset-request", invalidResetRequestContent);

            if (!invalidResetRequestResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Invalid email reset request should return success: {invalidResetRequestResponse.StatusCode}");
                return false;
            }

            // Get the reset token from the database (in a real app, this would come from email)
            var resetToken = await dbContext.PasswordResetTokens
                .Where(prt => prt.UserId == testUser.Id && !prt.IsUsed)
                .OrderByDescending(prt => prt.CreatedAt)
                .FirstOrDefaultAsync();

            if (resetToken == null)
            {
                Console.WriteLine("❌ Reset token was not created");
                return false;
            }

            // Test 3: Reset password with valid token
            Console.WriteLine("  ✅ Test 3: Reset password with valid token");
            var resetPassword = new
            {
                email = testUser.Email,
                token = resetToken.Token,
                newPassword = "NewPassword123!"
            };

            var resetPasswordJson = JsonSerializer.Serialize(resetPassword);
            var resetPasswordContent = new StringContent(resetPasswordJson, Encoding.UTF8, "application/json");
            var resetPasswordResponse = await client.PostAsync("/api/v1/auth/password/reset", resetPasswordContent);

            if (!resetPasswordResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Password reset failed: {resetPasswordResponse.StatusCode}");
                return false;
            }

            // Test 4: Verify password was actually changed by attempting login with new password
            Console.WriteLine("  ✅ Test 4: Verify password was changed");
            var loginRequest = new
            {
                username = testUser.Username,
                password = "NewPassword123!"
            };

            var loginJson = JsonSerializer.Serialize(loginRequest);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");
            var loginResponse = await client.PostAsync("/api/v1/auth/login", loginContent);

            if (!loginResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Login with new password failed: {loginResponse.StatusCode}");
                return false;
            }

            // Test 5: Verify old password no longer works
            Console.WriteLine("  ✅ Test 5: Verify old password no longer works");
            var oldPasswordLogin = new
            {
                username = testUser.Username,
                password = "OriginalPassword123!"
            };

            var oldPasswordJson = JsonSerializer.Serialize(oldPasswordLogin);
            var oldPasswordContent = new StringContent(oldPasswordJson, Encoding.UTF8, "application/json");
            var oldPasswordResponse = await client.PostAsync("/api/v1/auth/login", oldPasswordContent);

            if (oldPasswordResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("❌ Login with old password should have failed");
                return false;
            }

            // Test 6: Reset password with invalid token
            Console.WriteLine("  ✅ Test 6: Reset password with invalid token");
            var invalidTokenReset = new
            {
                email = testUser.Email,
                token = "invalid-token",
                newPassword = "AnotherPassword123!"
            };

            var invalidTokenJson = JsonSerializer.Serialize(invalidTokenReset);
            var invalidTokenContent = new StringContent(invalidTokenJson, Encoding.UTF8, "application/json");
            var invalidTokenResponse = await client.PostAsync("/api/v1/auth/password/reset", invalidTokenContent);

            if (invalidTokenResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("❌ Reset with invalid token should have failed");
                return false;
            }

            // Test 7: Try to reuse the same token (should fail)
            Console.WriteLine("  ✅ Test 7: Verify token cannot be reused");
            var reuseTokenReset = new
            {
                email = testUser.Email,
                token = resetToken.Token,
                newPassword = "YetAnotherPassword123!"
            };

            var reuseTokenJson = JsonSerializer.Serialize(reuseTokenReset);
            var reuseTokenContent = new StringContent(reuseTokenJson, Encoding.UTF8, "application/json");
            var reuseTokenResponse = await client.PostAsync("/api/v1/auth/password/reset", reuseTokenContent);

            if (reuseTokenResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("❌ Reusing token should have failed");
                return false;
            }

            Console.WriteLine("✅ All password reset endpoint tests passed!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Password reset endpoint test failed with exception: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }
}
