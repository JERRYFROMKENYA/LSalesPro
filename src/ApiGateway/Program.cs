using ApiGateway.Configuration;
using ApiGateway.Middleware;
using ApiGateway.Services;
using Serilog;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/apigateway-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "L-SalesPro API Gateway", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Configure authentication and CORS
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCorsConfiguration(builder.Configuration);

// Add authorization
builder.Services.AddAuthorization();

// Configure YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add health checks
builder.Services.AddHttpClient<IHealthCheckService, HealthCheckService>();
builder.Services.AddSingleton<IHealthCheckService, HealthCheckService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "L-SalesPro API Gateway v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

// Add middleware in the correct order
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseRouting();

// CORS must be after UseRouting and before UseAuthentication
app.UseCors();

// Authentication middleware
app.UseAuthentication();
app.UseMiddleware<JwtAuthenticationMiddleware>();
app.UseAuthorization();

// Health check endpoint
app.MapGet("/health", async (IHealthCheckService healthCheckService) =>
{
    var result = await healthCheckService.CheckHealthAsync();
    return Results.Ok(result);
})
.WithName("HealthCheck")
.WithTags("Health")
.Produces<HealthCheckResult>()
.AllowAnonymous();

// Gateway info endpoint
app.MapGet("/info", () =>
{
    return Results.Ok(new
    {
        service = "L-SalesPro API Gateway",
        version = "1.0.0",
        timestamp = DateTime.UtcNow,
        environment = app.Environment.EnvironmentName
    });
})
.WithName("GatewayInfo")
.WithTags("Info")
.AllowAnonymous();

// Map controllers for any direct gateway endpoints
app.MapControllers();

// Configure YARP reverse proxy
app.MapReverseProxy();

try
{
    Log.Information("Starting L-SalesPro API Gateway");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API Gateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
