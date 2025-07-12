using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace InventoryService.Api.Authorization;

public class PublicEndpointsRequirement : IAuthorizationRequirement
{
    public static bool IsPublicEndpoint(HttpContext context)
    {
        var publicPaths = new[]
        {
            "/api/v1/inventory/public",
            "/api/v1/inventory/info",
            "/api/v1/inventory/health",
            "/swagger",
            "/api-docs"
        };

        return publicPaths.Any(path => context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase));
    }
}
