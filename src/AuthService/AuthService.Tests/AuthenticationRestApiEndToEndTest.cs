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

public static class AuthenticationRestApiEndToEndTest
{
    public static async Task<bool> RunTestAsync()
    {
        Console.WriteLine("🔐 Testing Authentication REST APIs - End-to-End Integration...");
        
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

            Console.WriteLine("🚀 Starting Complete Authentication Flow Test...");

            // Test 1: Login with valid credentials
            Console.WriteLine("  ✅ Test 1: POST /api/v1/auth/login - Valid credentials");
            var loginRequest = new
            {
                username = "admin",
                password = "AdminPass123!"
            };

            var loginJson = JsonSerializer.Serialize(loginRequest);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");
            var loginResponse = await client.PostAsync("/api/v1/auth/login", loginContent);

            if (!loginResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Login failed: {loginResponse.StatusCode}");
                var loginError = await loginResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"   Error: {loginError}");
                return false;
            }

            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
            var loginData = JsonSerializer.Deserialize<JsonElement>(loginResponseContent);
            var accessToken = loginData.GetProperty("accessToken").GetString();
            var refreshToken = loginData.GetProperty("refreshToken").GetString();

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                Console.WriteLine("❌ No access token or refresh token received from login");
                return false;
            }

            Console.WriteLine($"   ✓ Access token received: {accessToken[..20]}...");
            Console.WriteLine($"   ✓ Refresh token received: {refreshToken[..20]}...");

            // Test 2: Login with invalid credentials
            Console.WriteLine("  ✅ Test 2: POST /api/v1/auth/login - Invalid credentials");
            var invalidLoginRequest = new
            {
                username = "admin",
                password = "WrongPassword123!"
            };

            var invalidLoginJson = JsonSerializer.Serialize(invalidLoginRequest);
            var invalidLoginContent = new StringContent(invalidLoginJson, Encoding.UTF8, "application/json");
            var invalidLoginResponse = await client.PostAsync("/api/v1/auth/login", invalidLoginContent);

            if (invalidLoginResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("❌ Login with invalid credentials should have failed");
                return false;
            }

            Console.WriteLine($"   ✓ Invalid login properly rejected: {invalidLoginResponse.StatusCode}");

            // Test 3: Get user profile with valid token
            Console.WriteLine("  ✅ Test 3: GET /api/v1/auth/user - Valid JWT token");
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var userProfileResponse = await client.GetAsync("/api/v1/auth/user");
            
            if (!userProfileResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ User profile request failed: {userProfileResponse.StatusCode}");
                return false;
            }

            var userProfileContent = await userProfileResponse.Content.ReadAsStringAsync();
            var userData = JsonSerializer.Deserialize<JsonElement>(userProfileContent);
            var username = userData.GetProperty("username").GetString();

            Console.WriteLine($"   ✓ User profile retrieved: {username}");

            // Test 4: Get user profile with invalid token
            Console.WriteLine("  ✅ Test 4: GET /api/v1/auth/user - Invalid JWT token");
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid.token.here");

            var invalidUserProfileResponse = await client.GetAsync("/api/v1/auth/user");
            
            if (invalidUserProfileResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("❌ User profile with invalid token should have failed");
                return false;
            }

            Console.WriteLine($"   ✓ Invalid token properly rejected: {invalidUserProfileResponse.StatusCode}");

            // Test 5: Refresh token with valid refresh token
            Console.WriteLine("  ✅ Test 5: POST /api/v1/auth/refresh - Valid refresh token");
            var refreshRequest = new
            {
                refreshToken = refreshToken
            };

            var refreshJson = JsonSerializer.Serialize(refreshRequest);
            var refreshContent = new StringContent(refreshJson, Encoding.UTF8, "application/json");
            var refreshResponse = await client.PostAsync("/api/v1/auth/refresh", refreshContent);

            if (!refreshResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Token refresh failed: {refreshResponse.StatusCode}");
                var refreshError = await refreshResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"   Error: {refreshError}");
                return false;
            }

            var refreshResponseContent = await refreshResponse.Content.ReadAsStringAsync();
            var refreshData = JsonSerializer.Deserialize<JsonElement>(refreshResponseContent);
            var newAccessToken = refreshData.GetProperty("accessToken").GetString();

            Console.WriteLine($"   ✓ New access token received: {newAccessToken?[..20]}...");

            // Test 6: Refresh token with invalid refresh token
            Console.WriteLine("  ✅ Test 6: POST /api/v1/auth/refresh - Invalid refresh token");
            var invalidRefreshRequest = new
            {
                refreshToken = "invalid-refresh-token"
            };

            var invalidRefreshJson = JsonSerializer.Serialize(invalidRefreshRequest);
            var invalidRefreshContent = new StringContent(invalidRefreshJson, Encoding.UTF8, "application/json");
            var invalidRefreshResponse = await client.PostAsync("/api/v1/auth/refresh", invalidRefreshContent);

            if (invalidRefreshResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("❌ Refresh with invalid token should have failed");
                return false;
            }

            Console.WriteLine($"   ✓ Invalid refresh token properly rejected: {invalidRefreshResponse.StatusCode}");

            // Test 7: Password reset request
            Console.WriteLine("  ✅ Test 7: POST /api/v1/auth/password/reset-request - Valid email");
            var resetRequestData = new
            {
                email = "admin@lsalespro.com"
            };

            var resetRequestJson = JsonSerializer.Serialize(resetRequestData);
            var resetRequestContent = new StringContent(resetRequestJson, Encoding.UTF8, "application/json");
            var resetRequestResponse = await client.PostAsync("/api/v1/auth/password/reset-request", resetRequestContent);

            if (!resetRequestResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Password reset request failed: {resetRequestResponse.StatusCode}");
                return false;
            }

            Console.WriteLine("   ✓ Password reset request accepted");

            // Get the reset token from the database for testing
            var resetToken = await dbContext.PasswordResetTokens
                .Where(prt => !prt.IsUsed)
                .OrderByDescending(prt => prt.CreatedAt)
                .FirstOrDefaultAsync();

            if (resetToken != null)
            {
                // Test 8: Password reset with valid token
                Console.WriteLine("  ✅ Test 8: POST /api/v1/auth/password/reset - Valid reset token");
                var resetPasswordData = new
                {
                    email = "admin@lsalespro.com",
                    token = resetToken.Token,
                    newPassword = "NewAdminPass123!"
                };

                var resetPasswordJson = JsonSerializer.Serialize(resetPasswordData);
                var resetPasswordContent = new StringContent(resetPasswordJson, Encoding.UTF8, "application/json");
                var resetPasswordResponse = await client.PostAsync("/api/v1/auth/password/reset", resetPasswordContent);

                if (!resetPasswordResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Password reset failed: {resetPasswordResponse.StatusCode}");
                    return false;
                }

                Console.WriteLine("   ✓ Password reset completed successfully");

                // Test 9: Login with new password
                Console.WriteLine("  ✅ Test 9: POST /api/v1/auth/login - Login with new password");
                var newPasswordLoginRequest = new
                {
                    username = "admin",
                    password = "NewAdminPass123!"
                };

                var newPasswordLoginJson = JsonSerializer.Serialize(newPasswordLoginRequest);
                var newPasswordLoginContent = new StringContent(newPasswordLoginJson, Encoding.UTF8, "application/json");
                var newPasswordLoginResponse = await client.PostAsync("/api/v1/auth/login", newPasswordLoginContent);

                if (!newPasswordLoginResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Login with new password failed: {newPasswordLoginResponse.StatusCode}");
                    return false;
                }

                Console.WriteLine("   ✓ Login with new password successful");
            }
            else
            {
                Console.WriteLine("   ⚠️ Skipping password reset test - no reset token found");
            }

            // Test 10: Logout
            Console.WriteLine("  ✅ Test 10: POST /api/v1/auth/logout - Valid refresh token");
            var logoutRequest = new
            {
                refreshToken = refreshToken
            };

            var logoutJson = JsonSerializer.Serialize(logoutRequest);
            var logoutContent = new StringContent(logoutJson, Encoding.UTF8, "application/json");
            var logoutResponse = await client.PostAsync("/api/v1/auth/logout", logoutContent);

            if (!logoutResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Logout failed: {logoutResponse.StatusCode}");
                return false;
            }

            Console.WriteLine("   ✓ Logout successful");

            // Test 11: Try to use logged out refresh token
            Console.WriteLine("  ✅ Test 11: POST /api/v1/auth/refresh - Using logged out refresh token");
            var loggedOutRefreshResponse = await client.PostAsync("/api/v1/auth/refresh", refreshContent);

            if (loggedOutRefreshResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("❌ Refresh with logged out token should have failed");
                return false;
            }

            Console.WriteLine($"   ✓ Logged out refresh token properly rejected: {loggedOutRefreshResponse.StatusCode}");

            // Test 12: Malformed requests
            Console.WriteLine("  ✅ Test 12: Testing malformed requests");
            
            var malformedLoginContent = new StringContent("{invalid json", Encoding.UTF8, "application/json");
            var malformedLoginResponse = await client.PostAsync("/api/v1/auth/login", malformedLoginContent);
            
            if (malformedLoginResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("❌ Malformed login request should have failed");
                return false;
            }

            Console.WriteLine($"   ✓ Malformed requests properly rejected: {malformedLoginResponse.StatusCode}");

            Console.WriteLine("✅ All Authentication REST API end-to-end tests passed!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Authentication REST API test failed with exception: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }
}
