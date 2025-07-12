using InventoryService.Api.Authorization;
using InventoryService.Application.Interfaces;
using InventoryService.Application.Services;
using InventoryService.Infrastructure.Data;
using InventoryService.Infrastructure.Repositories;
using InventoryService.Domain.Interfaces;
using InventoryService.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add gRPC services
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
});

// Add database context
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add repository dependencies
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
builder.Services.AddScoped<IStockReservationRepository, StockReservationRepository>();

// Add application services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IStockReservationService, StockReservationService>();
builder.Services.AddScoped<IMultiWarehouseStockService, MultiWarehouseStockService>();
builder.Services.AddScoped<ILowStockMonitoringService, LowStockMonitoringService>();
builder.Services.AddScoped<InventoryService.Application.Interfaces.IInventoryService, InventoryService.Application.Services.InventoryService>();

// Add Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Inventory Service API", 
        Version = "v1",
        Description = "L-SalesPro Inventory Management Microservice"
    });
    
    // Enable XML comments for API documentation
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// Add health checks
builder.Services.AddHealthChecks();

// Add authorization
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAssertion(context =>
            context.Resource is HttpContext httpContext &&
            PublicEndpointsRequirement.IsPublicEndpoint(httpContext))
        .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory Service API V1");
    });
}

app.UseHttpsRedirection();

app.UseRouting();

app.MapControllers();
app.MapHealthChecks("/health");

// Map gRPC services
app.MapGrpcService<InventoryGrpcService>();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    context.Database.EnsureCreated();
}

app.Run();

// Make the implicit Program class public for testing
public partial class Program { }
