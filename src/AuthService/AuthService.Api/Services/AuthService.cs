using Grpc.Core;
using Leysco.AuthService.Protos;
using AuthService.Application.Interfaces;
using AuthService.Application.DTOs;

namespace AuthService.Api.Services
{
    public class AuthService : Leysco.AuthService.Protos.AuthService.AuthServiceBase
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUserService _userService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IJwtTokenService jwtTokenService, IUserService userService, ILogger<AuthService> logger)
        {
            _jwtTokenService = jwtTokenService;
            _userService = userService;
            _logger = logger;
        }

        public override async Task<ValidateTokenResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Validating token for: {Token}", request.Token);
            var isValid = _jwtTokenService.ValidateToken(request.Token);
            return new ValidateTokenResponse { IsValid = isValid };
        }

        public override async Task<GetUserPermissionsResponse> GetUserPermissions(GetUserPermissionsRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Getting permissions for user ID: {UserId}", request.UserId);
            var permissions = await _userService.GetUserPermissionsAsync(request.UserId);
            var response = new GetUserPermissionsResponse();
            response.Permissions.AddRange(permissions);
            return response;
        }

        public override async Task<GetUserByIdResponse> GetUserById(GetUserByIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Getting user by ID: {UserId}", request.UserId);
            var user = await _userService.GetUserByIdAsync(Guid.Parse(request.UserId));
            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"User with ID {request.UserId} not found."));
            }

            return new GetUserByIdResponse
            {
                UserId = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role
            };
        }
    }
}
