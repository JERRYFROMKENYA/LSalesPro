using Grpc.Core;
using Shared.Contracts.Auth;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using Microsoft.Extensions.Logging;

namespace AuthService.Api.Services;

public class AuthGrpcService : Shared.Contracts.Auth.AuthService.AuthServiceBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthGrpcService> _logger;

    public AuthGrpcService(
        IAuthService authService,
        IUserService userService,
        IJwtTokenService jwtTokenService,
        ILogger<AuthGrpcService> logger)
    {
        _authService = authService;
        _userService = userService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public override async Task<ValidateTokenResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC ValidateToken called for token: {Token}", request.Token[..10] + "...");

        try
        {
            var isValid = await _authService.ValidateTokenAsync(request.Token);
            
            if (!isValid)
            {
                _logger.LogWarning("Token validation failed");
                return new ValidateTokenResponse
                {
                    IsValid = false,
                    ErrorMessage = "Invalid or expired token"
                };
            }

            // Extract user information from the token
            var principal = await _jwtTokenService.ValidateTokenAsync(request.Token);
            if (principal == null)
            {
                return new ValidateTokenResponse
                {
                    IsValid = false,
                    ErrorMessage = "Unable to extract user information from token"
                };
            }

            var userId = principal.FindFirst("sub")?.Value ?? principal.FindFirst("id")?.Value;
            var username = principal.FindFirst("username")?.Value ?? principal.Identity?.Name;
            var roles = principal.FindAll("role").Select(c => c.Value).ToArray();

            _logger.LogInformation("Token validated successfully for user: {UserId}", userId);

            return new ValidateTokenResponse
            {
                IsValid = true,
                UserId = userId ?? string.Empty,
                Username = username ?? string.Empty,
                Roles = { roles }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token via gRPC");
            return new ValidateTokenResponse
            {
                IsValid = false,
                ErrorMessage = "Internal server error during token validation"
            };
        }
    }

    public override async Task<GetUserPermissionsResponse> GetUserPermissions(GetUserPermissionsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC GetUserPermissions called for user: {UserId}", request.UserId);

        try
        {
            if (!Guid.TryParse(request.UserId, out var userId))
            {
                return new GetUserPermissionsResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid user ID format"
                };
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return new GetUserPermissionsResponse
                {
                    Success = false,
                    ErrorMessage = "User not found"
                };
            }

            var permissions = user.Roles?.SelectMany(r => r.Permissions?.Select(p => p.Name) ?? Enumerable.Empty<string>())
                                 .Distinct()
                                 .ToArray() ?? Array.Empty<string>();

            var roles = user.Roles?.Select(r => r.Name).ToArray() ?? Array.Empty<string>();

            _logger.LogInformation("Retrieved {PermissionCount} permissions and {RoleCount} roles for user: {UserId}", 
                permissions.Length, roles.Length, request.UserId);

            return new GetUserPermissionsResponse
            {
                Success = true,
                Permissions = { permissions },
                Roles = { roles }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user permissions via gRPC for user: {UserId}", request.UserId);
            return new GetUserPermissionsResponse
            {
                Success = false,
                ErrorMessage = "Internal server error while retrieving user permissions"
            };
        }
    }

    public override async Task<GetUserByIdResponse> GetUserById(GetUserByIdRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC GetUserById called for user: {UserId}", request.UserId);

        try
        {
            if (!Guid.TryParse(request.UserId, out var userId))
            {
                return new GetUserByIdResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid user ID format"
                };
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return new GetUserByIdResponse
                {
                    Success = false,
                    ErrorMessage = "User not found"
                };
            }

            var userInfo = new UserInfo
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                Roles = { user.Roles?.Select(r => r.Name) ?? Enumerable.Empty<string>() }
            };

            _logger.LogInformation("Retrieved user information for: {UserId}", request.UserId);

            return new GetUserByIdResponse
            {
                Success = true,
                User = userInfo
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID via gRPC for user: {UserId}", request.UserId);
            return new GetUserByIdResponse
            {
                Success = false,
                ErrorMessage = "Internal server error while retrieving user information"
            };
        }
    }
}
