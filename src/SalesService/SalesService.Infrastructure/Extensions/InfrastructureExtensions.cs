using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SalesService.Application.Interfaces;
using SalesService.Infrastructure.Data;
using SalesService.Infrastructure.Repositories;

namespace SalesService.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<SalesDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("SalesConnection") ?? "Data Source=SalesService.db",
                b => b.MigrationsAssembly(typeof(SalesDbContext).Assembly.FullName)));

        // Register repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<SalesService.Domain.Interfaces.INotificationRepository, NotificationRepository>();
        
        // Register database initializer
        services.AddScoped<ISalesDbInitializer, SalesDbInitializer>();

        return services;
    }
}
