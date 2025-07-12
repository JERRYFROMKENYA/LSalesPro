using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace AuthService.Api.Authorization;

public class PublicEndpointsRequirement : IAuthorizationRequirement
{
    public static bool IsPublicEndpoint(HttpContext context)
    {
        var publicPaths = new[]
        {
            "/api/v1/auth/register",
            "/api/v1/auth/login",
            "/api/v1/auth/refresh",
            "/api/v1/auth/password/reset-request",
            "/api/v1/auth/password/reset",
            "/health",
            "/info",
            "/swagger",
            "/api-docs"
        };

        return publicPaths.Any(path => context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase));
    }
}
