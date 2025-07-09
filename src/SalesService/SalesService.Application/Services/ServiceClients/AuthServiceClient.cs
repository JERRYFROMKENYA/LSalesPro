using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SalesService.Application.DTOs;
using SalesService.Application.Interfaces;
using SalesService.Application.Settings;
using Shared.Contracts;
using Shared.Contracts.Auth;

namespace SalesService.Application.Services.ServiceClients;

public class AuthServiceClient : IAuthServiceClient, IDisposable
{
    private readonly GrpcChannel _channel;
    private readonly Shared.Contracts.Auth.AuthService.AuthServiceClient _client;
    private readonly ILogger<AuthServiceClient> _logger;
    private bool _disposed;

    public AuthServiceClient(IOptions<ServiceSettings> settings, ILogger<AuthServiceClient> logger)
    {
        _logger = logger;
        
        try
        {
            var authServiceUrl = settings.Value.AuthServiceUrl;
            if (string.IsNullOrEmpty(authServiceUrl))
            {
                throw new InvalidOperationException("AuthService URL not configured");
            }

            _channel = GrpcChannel.ForAddress(authServiceUrl);
            _client = new Shared.Contracts.Auth.AuthService.AuthServiceClient(_channel);
            
            _logger.LogInformation("AuthServiceClient initialized with URL: {Url}", authServiceUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize AuthServiceClient");
            throw;
        }
    }

    public async Task<UserValidationResultDto> ValidateTokenAsync(string token)
    {
        try
        {
            var request = new ValidateTokenRequest
            {
                Token = token
            };

            var response = await _client.ValidateTokenAsync(request);

            return new UserValidationResultDto
            {
                IsValid = response.IsValid,
                UserId = response.UserId,
                Username = response.Username,
                Roles = response.Roles.ToList(),
                Permissions = new List<string>(), // Proto doesn't include permissions
                ExpiresAt = DateTime.Now.AddHours(1) // Proto doesn't include expiry, defaulting to 1 hour
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error validating token");
            
            return new UserValidationResultDto
            {
                IsValid = false
            };
        }
    }

    public async Task<UserDetailsDto?> GetUserByIdAsync(string userId)
    {
        try
        {
            var request = new GetUserByIdRequest
            {
                UserId = userId
            };

            var response = await _client.GetUserByIdAsync(request);

            if (response == null || response.User == null || string.IsNullOrEmpty(response.User.Id))
            {
                return null;
            }

            return new UserDetailsDto
            {
                Id = response.User.Id,
                Username = response.User.Username,
                Email = response.User.Email,
                FirstName = response.User.FirstName,
                LastName = response.User.LastName,
                IsActive = true, // No IsActive field in UserInfo proto, defaulting to true
                Roles = response.User.Roles.ToList()
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error getting user details for UserId: {UserId}", userId);
            return null;
        }
    }

    public async Task<List<string>> GetUserPermissionsAsync(string userId)
    {
        try
        {
            var request = new GetUserPermissionsRequest
            {
                UserId = userId
            };

            var response = await _client.GetUserPermissionsAsync(request);
            
            return response.Permissions.ToList();
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error getting user permissions for UserId: {UserId}", userId);
            return new List<string>();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _channel?.Dispose();
        }

        _disposed = true;
    }
}
