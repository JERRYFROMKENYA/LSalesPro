using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Xunit;

namespace AuthService.Tests;

public class PasswordResetEndpointTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public PasswordResetEndpointTest(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RequestPasswordReset_WithValidEmail_ReturnsOk()
    {
        // Arrange
        var resetRequest = new { email = "resetuser@example.com" };
        var resetRequestJson = JsonSerializer.Serialize(resetRequest);
        var resetRequestContent = new StringContent(resetRequestJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/auth/password/reset-request", resetRequestContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_ReturnsOk()
    {
        // Arrange
        var scopeFactory = _factory.Services.GetService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var user = await dbContext.Users.FirstAsync(u => u.Email == "resetuser@example.com");
        var resetToken = new PasswordResetToken { UserId = user.Id, Token = Guid.NewGuid().ToString(), ExpiresAt = DateTime.UtcNow.AddHours(1) };
        dbContext.PasswordResetTokens.Add(resetToken);
        await dbContext.SaveChangesAsync();
        var resetPassword = new { email = user.Email, token = resetToken.Token, newPassword = "NewPassword123!" };
        var resetPasswordJson = JsonSerializer.Serialize(resetPassword);
        var resetPasswordContent = new StringContent(resetPasswordJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/auth/password/reset", resetPasswordContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var resetPassword = new { email = "resetuser@example.com", token = "invalid-token", newPassword = "AnotherPassword123!" };
        var resetPasswordJson = JsonSerializer.Serialize(resetPassword);
        var resetPasswordContent = new StringContent(resetPasswordJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/auth/password/reset", resetPasswordContent);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
