using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using AuthService.Infrastructure.Data;
using AuthService.Domain.Entities;
using AuthService.Application.Interfaces;

namespace AuthService.Tests;

public static class AuthenticationMiddlewareTest
{
    public static async Task<bool> RunTestAsync()
    {
        Console.WriteLine("🔐 Testing Authentication Middleware Configuration...");
        
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

            // Ensure database is created and seeded
            await dbContext.Database.EnsureCreatedAsync();

            // Test 1: Access protected endpoint without token (should return 401)
            Console.WriteLine("  ✅ Test 1: Access protected endpoint without authentication");
            var unauthorizedResponse = await client.GetAsync("/api/v1/auth/user");
            
            if (unauthorizedResponse.StatusCode != System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine($"❌ Expected 401 Unauthorized, got: {unauthorizedResponse.StatusCode}");
                return false;
            }

            // Test 2: Login to get a valid token
            Console.WriteLine("  ✅ Test 2: Login to obtain JWT token");
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
                return false;
            }

            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
            var loginData = JsonSerializer.Deserialize<JsonElement>(loginResponseContent);
            var accessToken = loginData.GetProperty("accessToken").GetString();

            if (string.IsNullOrEmpty(accessToken))
            {
                Console.WriteLine("❌ No access token received from login");
                return false;
            }

            // Test 3: Access protected endpoint with valid token (should return 200)
            Console.WriteLine("  ✅ Test 3: Access protected endpoint with valid JWT token");
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var authorizedResponse = await client.GetAsync("/api/v1/auth/user");
            
            if (!authorizedResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Expected 200 OK with valid token, got: {authorizedResponse.StatusCode}");
                return false;
            }

            // Test 4: Access protected endpoint with invalid token (should return 401)
            Console.WriteLine("  ✅ Test 4: Access protected endpoint with invalid JWT token");
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid.token.here");

            var invalidTokenResponse = await client.GetAsync("/api/v1/auth/user");
            
            if (invalidTokenResponse.StatusCode != System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine($"❌ Expected 401 Unauthorized with invalid token, got: {invalidTokenResponse.StatusCode}");
                return false;
            }

            // Test 5: Check that Swagger UI is accessible (no auth required)
            Console.WriteLine("  ✅ Test 5: Verify Swagger UI is accessible");
            var swaggerResponse = await client.GetAsync("/swagger/index.html");
            
            if (!swaggerResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Swagger UI not accessible: {swaggerResponse.StatusCode}");
                return false;
            }

            // Test 6: Check that Swagger JSON includes authentication scheme
            Console.WriteLine("  ✅ Test 6: Verify Swagger JSON includes authentication configuration");
            var swaggerJsonResponse = await client.GetAsync("/swagger/v1/swagger.json");
            
            if (!swaggerJsonResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Swagger JSON not accessible: {swaggerJsonResponse.StatusCode}");
                return false;
            }

            var swaggerContent = await swaggerJsonResponse.Content.ReadAsStringAsync();
            if (!swaggerContent.Contains("Bearer") || !swaggerContent.Contains("JWT"))
            {
                Console.WriteLine("❌ Swagger JSON doesn't contain JWT authentication configuration");
                return false;
            }

            Console.WriteLine("✅ All authentication middleware tests passed!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Authentication middleware test failed with exception: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }
}
