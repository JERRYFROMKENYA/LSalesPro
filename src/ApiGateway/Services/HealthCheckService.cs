namespace ApiGateway.Services;

public interface IHealthCheckService
{
    Task<HealthCheckResult> CheckHealthAsync();
    Task<HealthCheckResult> CheckServiceHealthAsync(string serviceName, string serviceUrl);
}

public class HealthCheckService : IHealthCheckService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HealthCheckService> _logger;
    private readonly IConfiguration _configuration;

    public HealthCheckService(HttpClient httpClient, ILogger<HealthCheckService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync()
    {
        var results = new List<ServiceHealthResult>();

        // Check Authentication Service
        var authResult = await CheckServiceHealthAsync("AuthService", "https://localhost:5001");
        results.Add(new ServiceHealthResult { ServiceName = "AuthService", IsHealthy = authResult.IsHealthy, Message = authResult.Message });

        // Check Inventory Service  
        var inventoryResult = await CheckServiceHealthAsync("InventoryService", "https://localhost:5003");
        results.Add(new ServiceHealthResult { ServiceName = "InventoryService", IsHealthy = inventoryResult.IsHealthy, Message = inventoryResult.Message });

        // Check Sales Service
        var salesResult = await CheckServiceHealthAsync("SalesService", "https://localhost:5005");
        results.Add(new ServiceHealthResult { ServiceName = "SalesService", IsHealthy = salesResult.IsHealthy, Message = salesResult.Message });

        var overallHealth = results.All(r => r.IsHealthy);

        return new HealthCheckResult
        {
            IsHealthy = overallHealth,
            Message = overallHealth ? "All services are healthy" : "One or more services are unhealthy",
            Services = results,
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task<HealthCheckResult> CheckServiceHealthAsync(string serviceName, string serviceUrl)
    {
        try
        {
            _logger.LogDebug("Checking health for service {ServiceName} at {ServiceUrl}", serviceName, serviceUrl);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var response = await _httpClient.GetAsync($"{serviceUrl}/health", cts.Token);

            var isHealthy = response.IsSuccessStatusCode;
            var message = isHealthy ? "Service is healthy" : $"Service returned {response.StatusCode}";

            _logger.LogDebug("Health check for {ServiceName}: {IsHealthy} - {Message}", serviceName, isHealthy, message);

            return new HealthCheckResult
            {
                IsHealthy = isHealthy,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Health check timeout for service {ServiceName}", serviceName);
            return new HealthCheckResult
            {
                IsHealthy = false,
                Message = "Service health check timeout",
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed for service {ServiceName}", serviceName);
            return new HealthCheckResult
            {
                IsHealthy = false,
                Message = $"Health check failed: {ex.Message}",
                Timestamp = DateTime.UtcNow
            };
        }
    }
}

public class HealthCheckResult
{
    public bool IsHealthy { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public List<ServiceHealthResult> Services { get; set; } = new();
}

public class ServiceHealthResult
{
    public string ServiceName { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public string Message { get; set; } = string.Empty;
}
