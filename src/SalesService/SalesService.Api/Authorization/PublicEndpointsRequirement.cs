using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace SalesService.Api.Authorization;

public class PublicEndpointsRequirement : IAuthorizationRequirement
{
    public static bool IsPublicEndpoint(HttpContext context)
    {
        var publicPaths = new[]
        {
            "/api/v1/sales/public",
            "/api/v1/sales/info",
            "/api/v1/sales/health",
            "/swagger",
            "/api-docs"
        };

        return publicPaths.Any(path => context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase));
    }
}
