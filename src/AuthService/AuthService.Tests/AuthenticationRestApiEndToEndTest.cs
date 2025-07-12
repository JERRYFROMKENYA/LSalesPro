using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AuthService.Infrastructure.Data;
using Xunit;

namespace AuthService.Tests;

public class AuthenticationRestApiEndToEndTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthenticationRestApiEndToEndTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new
        {
            username = "admin",
            password = "AdminPass123!"
        };
        var loginJson = JsonSerializer.Serialize(loginRequest);
        var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/v1/auth/login", loginContent);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var loginData = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.NotEmpty(loginData.GetProperty("accessToken").GetString());
        Assert.NotEmpty(loginData.GetProperty("refreshToken").GetString());
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new
        {
            username = "admin",
            password = "WrongPassword123!"
        };
        var loginJson = JsonSerializer.Serialize(loginRequest);
        var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/v1/auth/login", loginContent);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUserProfile_WithValidToken_ReturnsUserProfile()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/auth/user");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var userData = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.Equal("admin", userData.GetProperty("username").GetString());
    }

    [Fact]
    public async Task RefreshToken_WithValidRefreshToken_ReturnsNewTokens()
    {
        // Arrange
        var client = _factory.CreateClient();
        var refreshToken = await GetRefreshTokenAsync(client);
        var refreshRequest = new { refreshToken };
        var refreshJson = JsonSerializer.Serialize(refreshRequest);
        var refreshContent = new StringContent(refreshJson, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/v1/auth/refresh", refreshContent);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var refreshData = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.NotEmpty(refreshData.GetProperty("accessToken").GetString());
        Assert.NotEmpty(refreshData.GetProperty("refreshToken").GetString());
    }

    private async Task<HttpClient> GetAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var loginRequest = new
        {
            username = "admin",
            password = "AdminPass123!"
        };
        var loginJson = JsonSerializer.Serialize(loginRequest);
        var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");
        var loginResponse = await client.PostAsync("/api/v1/auth/login", loginContent);
        var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
        var loginData = JsonSerializer.Deserialize<JsonElement>(loginResponseContent);
        var accessToken = loginData.GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    private async Task<string> GetRefreshTokenAsync(HttpClient client)
    {
        var loginRequest = new
        {
            username = "admin",
            password = "AdminPass123!"
        };
        var loginJson = JsonSerializer.Serialize(loginRequest);
        var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");
        var loginResponse = await client.PostAsync("/api/v1/auth/login", loginContent);
        var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
        var loginData = JsonSerializer.Deserialize<JsonElement>(loginResponseContent);
        return loginData.GetProperty("refreshToken").GetString();
    }
}
