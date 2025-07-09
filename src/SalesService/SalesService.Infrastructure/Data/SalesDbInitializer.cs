using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SalesService.Domain.Entities;

namespace SalesService.Infrastructure.Data;

public class SalesDbInitializer : ISalesDbInitializer
{
    private readonly SalesDbContext _dbContext;
    private readonly ILogger<SalesDbInitializer> _logger;

    public SalesDbInitializer(SalesDbContext dbContext, ILogger<SalesDbInitializer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Apply pending migrations
            _logger.LogInformation("Applying migrations...");
            await _dbContext.Database.MigrateAsync();
            
            // Seed data
            await SeedDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }

    public async Task SeedDataAsync()
    {
        try
        {
            // Seed customers if none exist
            if (!await _dbContext.Customers.AnyAsync())
            {
                _logger.LogInformation("Seeding customers...");
                var customers = GetSeedCustomers();
                await _dbContext.Customers.AddRangeAsync(customers);
                await _dbContext.SaveChangesAsync();
            }

            // Seed sample orders if none exist
            if (!await _dbContext.Orders.AnyAsync())
            {
                _logger.LogInformation("Seeding orders...");
                // Get customers to associate with orders
                var customers = await _dbContext.Customers.ToListAsync();
                if (customers.Any())
                {
                    var orders = GetSeedOrders(customers);
                    await _dbContext.Orders.AddRangeAsync(orders);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding data");
            throw;
        }
    }

    private List<Customer> GetSeedCustomers()
    {
        return new List<Customer>
        {
            new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "555-123-4567",
                Address = "123 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "10001",
                CustomerType = "Individual",
                CustomerCategory = "Regular",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                PhoneNumber = "555-987-6543",
                Address = "456 Oak Ave",
                City = "Los Angeles",
                State = "CA",
                ZipCode = "90001",
                CustomerType = "Individual",
                CustomerCategory = "Premium",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "Acme",
                LastName = "Corp",
                Email = "contact@acmecorp.com",
                PhoneNumber = "555-789-0123",
                Address = "789 Business Blvd",
                City = "Chicago",
                State = "IL",
                ZipCode = "60007",
                CustomerType = "Business",
                CustomerCategory = "Enterprise",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
    }

    private List<Order> GetSeedOrders(List<Customer> customers)
    {
        var orders = new List<Order>();

        // Random for generating sample data
        var random = new Random();
        
        // List of possible order statuses
        var statuses = new[] { "Pending", "Confirmed", "Processing", "Shipped", "Delivered", "Cancelled" };
        
        // Create 1-3 orders for each customer
        foreach (var customer in customers)
        {
            var orderCount = random.Next(1, 4); // 1-3 orders per customer
            
            for (int i = 0; i < orderCount; i++)
            {
                var orderDate = DateTime.UtcNow.AddDays(-random.Next(1, 30)); // Order from last 30 days
                var orderStatus = statuses[random.Next(statuses.Length)];
                
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    OrderNumber = $"ORD-{DateTime.UtcNow.Year}-{10000 + random.Next(90000)}",
                    OrderDate = orderDate,
                    Status = orderStatus,
                    ShippingAddress = customer.Address,
                    ShippingCity = customer.City,
                    ShippingState = customer.State,
                    ShippingZipCode = customer.ZipCode,
                    BillingAddress = customer.Address,
                    BillingCity = customer.City,
                    BillingState = customer.State,
                    BillingZipCode = customer.ZipCode,
                    PaymentMethod = "Credit Card",
                    TotalAmount = 0, // Will be calculated from items
                    Notes = "Sample order created by seeder",
                    CreatedAt = orderDate,
                    UpdatedAt = orderDate
                };
                
                // Add 1-5 random order items
                var itemCount = random.Next(1, 6);
                for (int j = 0; j < itemCount; j++)
                {
                    var price = random.Next(10, 500);
                    var quantity = random.Next(1, 5);
                    var discount = random.Next(0, 20);
                    var lineTotal = price * quantity * (1 - discount / 100.0m);
                    
                    var item = new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        ProductSku = $"SKU-{random.Next(10000, 99999)}",
                        ProductName = $"Sample Product {j + 1}",
                        Quantity = quantity,
                        UnitPrice = price,
                        Discount = discount,
                        LineTotal = lineTotal,
                        CreatedAt = orderDate,
                        UpdatedAt = orderDate
                    };
                    
                    order.OrderItems = order.OrderItems ?? new List<OrderItem>();
                    order.OrderItems.Add(item);
                    order.TotalAmount += lineTotal;
                }
                
                orders.Add(order);
            }
        }
        
        return orders;
    }
}
