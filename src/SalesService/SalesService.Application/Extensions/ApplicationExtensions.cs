using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SalesService.Application.Interfaces;
using SalesService.Application.Services;
using SalesService.Application.Services.ServiceClients;
using SalesService.Application.Settings;

namespace SalesService.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        // Register application services
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IReportsService, ReportsService>();
        
        // Register service clients
        services.AddScoped<IInventoryServiceClient, InventoryServiceClient>();
        services.AddScoped<IAuthServiceClient, AuthServiceClient>();
        
        // Register service settings
        services.Configure<ServiceSettings>(configuration.GetSection("ServiceSettings"));
        
        return services;
    }
}
