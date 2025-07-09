using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get()
    {
        return Ok(new
        {
            service = "L-SalesPro API Gateway",
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    [HttpGet("detailed")]
    [AllowAnonymous]
    public IActionResult GetDetailed()
    {
        return Ok(new
        {
            service = "L-SalesPro API Gateway",
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            uptime = Environment.TickCount64,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            routes = new[]
            {
                "/api/auth/* -> Auth Service",
                "/api/inventory/* -> Inventory Service", 
                "/api/sales/* -> Sales Service"
            }
        });
    }
}
