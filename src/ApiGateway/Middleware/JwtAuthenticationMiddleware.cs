using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Text.Json;

namespace ApiGateway.Middleware;

public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtAuthenticationMiddleware> _logger;

    public JwtAuthenticationMiddleware(RequestDelegate next, ILogger<JwtAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip authentication for certain paths
        if (ShouldSkipAuthentication(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // Extract JWT token from Authorization header
        var token = ExtractTokenFromHeader(context.Request);
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Missing or invalid Authorization header for path: {Path}", context.Request.Path);
            await HandleUnauthorized(context, "Missing authorization token");
            return;
        }

        try
        {
            // The JWT authentication is handled by the built-in middleware
            // This middleware adds additional logging and context
            _logger.LogDebug("Processing request with JWT token for path: {Path}", context.Request.Path);
            
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing authenticated request for path: {Path}", context.Request.Path);
            await HandleInternalError(context, "Authentication processing error");
        }
    }

    private static bool ShouldSkipAuthentication(PathString path)
    {
        var publicPaths = new[]
        {
            "/api/auth/login",
            "/api/auth/register", 
            "/api/auth/refresh",
            "/api/v1/auth/login",
            "/api/v1/auth/register",
            "/api/v1/auth/refresh",
            "/health",
            "/api/health",
            "/info",
            "/swagger",
            "/api-docs"
        };

        return publicPaths.Any(publicPath => path.StartsWithSegments(publicPath, StringComparison.OrdinalIgnoreCase));
    }

    private static string? ExtractTokenFromHeader(HttpRequest request)
    {
        var authHeader = request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return authHeader["Bearer ".Length..];
    }

    private static async Task HandleUnauthorized(HttpContext context, string message)
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Unauthorized",
            message = message,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static async Task HandleInternalError(HttpContext context, string message)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Internal Server Error",
            message = message,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
