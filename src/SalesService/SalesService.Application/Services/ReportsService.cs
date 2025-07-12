using AutoMapper;
using Microsoft.Extensions.Logging;
using SalesService.Application.DTOs;
using SalesService.Application.Interfaces;
using SalesService.Domain.Entities;

namespace SalesService.Application.Services;

public class ReportsService : IReportsService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<ReportsService> _logger;
    private readonly IMapper _mapper;

    public ReportsService(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        ILogger<ReportsService> logger,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<SalesSummaryReportDto> GetSalesSummaryReportAsync(DateRangeDto dateRange)
    {
        try
        {
            // Get all orders within date range
            var allOrders = await _orderRepository.GetAllAsync();
            var ordersInRange = allOrders.Where(o => 
                o.OrderDate >= dateRange.StartDate && 
                o.OrderDate <= dateRange.EndDate &&
                o.Status != "Cancelled").ToList();

            // Calculate summary metrics
            var totalSales = ordersInRange.Sum(o => o.TotalAmount);
            var totalOrders = ordersInRange.Count;
            var avgOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;
            var maxOrderValue = ordersInRange.Any() ? ordersInRange.Max(o => o.TotalAmount) : 0;
            var minOrderValue = ordersInRange.Any() ? ordersInRange.Min(o => o.TotalAmount) : 0;

            // Get status breakdown
            var statusCounts = ordersInRange
                .GroupBy(o => o.Status)
                .Select(g => new OrderStatusSummaryDto 
                { 
                    Status = g.Key, 
                    Count = g.Count(), 
                    TotalAmount = g.Sum(o => o.TotalAmount) 
                })
                .ToList();

            // Count unique customers
            var uniqueCustomers = ordersInRange.Select(o => o.CustomerId).Distinct().Count();

            return new SalesSummaryReportDto
            {
                StartDate = dateRange.StartDate,
                EndDate = dateRange.EndDate,
                TotalSales = totalSales,
                TotalOrders = totalOrders,
                AverageOrderValue = avgOrderValue,
                MaximumOrderValue = maxOrderValue,
                MinimumOrderValue = minOrderValue,
                UniqueCustomers = uniqueCustomers,
                OrderStatusBreakdown = statusCounts,
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sales summary report for period {StartDate} to {EndDate}", 
                dateRange.StartDate, dateRange.EndDate);
            throw;
        }
    }

    public async Task<List<DailySalesReportDto>> GetDailySalesReportAsync(DateRangeDto dateRange)
    {
        try
        {
            // Get all orders within date range
            var allOrders = await _orderRepository.GetAllAsync();
            var ordersInRange = allOrders.Where(o => 
                o.OrderDate >= dateRange.StartDate && 
                o.OrderDate <= dateRange.EndDate &&
                o.Status != "Cancelled").ToList();

            // Group orders by date
            var dailySales = ordersInRange
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new DailySalesReportDto
                {
                    Date = g.Key,
                    TotalSales = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count(),
                    AverageOrderValue = g.Count() > 0 ? g.Sum(o => o.TotalAmount) / g.Count() : 0
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Fill in missing dates with zero values
            var allDates = Enumerable.Range(0, (dateRange.EndDate - dateRange.StartDate).Days + 1)
                .Select(offset => dateRange.StartDate.AddDays(offset).Date);

            var result = allDates
                .GroupJoin(
                    dailySales,
                    date => date,
                    sale => sale.Date,
                    (date, sales) => sales.FirstOrDefault() ?? new DailySalesReportDto
                    {
                        Date = date,
                        TotalSales = 0,
                        OrderCount = 0,
                        AverageOrderValue = 0
                    }
                )
                .OrderBy(d => d.Date)
                .ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating daily sales report for period {StartDate} to {EndDate}", 
                dateRange.StartDate, dateRange.EndDate);
            throw;
        }
    }

    public async Task<List<MonthlySalesReportDto>> GetMonthlySalesReportAsync(int year)
    {
        try
        {
            // Get all orders for the specified year
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);
            
            var allOrders = await _orderRepository.GetAllAsync();
            var ordersInYear = allOrders.Where(o => 
                o.OrderDate >= startDate && 
                o.OrderDate <= endDate &&
                o.Status != "Cancelled").ToList();

            // Group orders by month
            var monthlySales = ordersInYear
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new MonthlySalesReportDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalSales = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count(),
                    AverageOrderValue = g.Count() > 0 ? g.Sum(o => o.TotalAmount) / g.Count() : 0
                })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ToList();

            // Fill in missing months with zero values
            var allMonths = Enumerable.Range(1, 12)
                .Select(month => new { Year = year, Month = month });

            var result = allMonths
                .GroupJoin(
                    monthlySales,
                    month => new { month.Year, month.Month },
                    sale => new { sale.Year, sale.Month },
                    (month, sales) => sales.FirstOrDefault() ?? new MonthlySalesReportDto
                    {
                        Year = month.Year,
                        Month = month.Month,
                        TotalSales = 0,
                        OrderCount = 0,
                        AverageOrderValue = 0
                    }
                )
                .OrderBy(d => d.Month)
                .ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating monthly sales report for year {Year}", year);
            throw;
        }
    }

    public async Task<List<ProductSalesReportDto>> GetTopSellingProductsAsync(DateRangeDto dateRange, int limit = 10)
    {
        try
        {
            // Get all orders within date range
            var allOrders = await _orderRepository.GetAllAsync();
            var ordersInRange = allOrders.Where(o => 
                o.OrderDate >= dateRange.StartDate && 
                o.OrderDate <= dateRange.EndDate &&
                o.Status != "Cancelled").ToList();

            // Flatten orders to order items
            var orderItems = ordersInRange
                .SelectMany(o => o.OrderItems)
                .ToList();

            // Group by product and calculate totals
            var productSales = orderItems
                .GroupBy(i => i.ProductSku)
                .Select(g => new ProductSalesReportDto
                {
                    ProductSku = g.Key,
                    ProductName = g.First().ProductName,
                    QuantitySold = g.Sum(i => i.Quantity),
                    TotalRevenue = g.Sum(i => i.LineTotal),
                    AverageUnitPrice = g.Sum(i => i.LineTotal) / g.Sum(i => i.Quantity),
                    OrderCount = g.Select(i => i.OrderId).Distinct().Count()
                })
                .OrderByDescending(p => p.TotalRevenue)
                .Take(limit)
                .ToList();

            return productSales;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating top selling products report for period {StartDate} to {EndDate}", 
                dateRange.StartDate, dateRange.EndDate);
            throw;
        }
    }

    public async Task<List<CustomerSalesReportDto>> GetTopCustomersAsync(DateRangeDto dateRange, int limit = 10)
    {
        try
        {
            // Get all customers
            var allCustomers = await _customerRepository.GetAllAsync();
            
            // Get all orders within date range
            var allOrders = await _orderRepository.GetAllAsync();
            var ordersInRange = allOrders.Where(o => 
                o.OrderDate >= dateRange.StartDate && 
                o.OrderDate <= dateRange.EndDate &&
                o.Status != "Cancelled").ToList();

            // Group by customer and calculate totals
            var customerSales = ordersInRange
                .GroupBy(o => o.CustomerId)
                .Select(g => {
                    var customer = allCustomers.FirstOrDefault(c => c.Id == g.Key);
                    return new CustomerSalesReportDto
                    {
                        CustomerId = g.Key,
                        CustomerName = customer?.Name ?? "Unknown Customer",
                        CustomerType = customer?.CustomerType ?? "Unknown",
                        CustomerCategory = customer?.CustomerCategory ?? "Unknown",
                        TotalSpent = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count(),
                        AverageOrderValue = g.Sum(o => o.TotalAmount) / g.Count(),
                        LastOrderDate = g.Max(o => o.OrderDate)
                    };
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(limit)
                .ToList();

            return customerSales;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating top customers report for period {StartDate} to {EndDate}", 
                dateRange.StartDate, dateRange.EndDate);
            throw;
        }
    }

    public async Task<CustomerActivityReportDto> GetCustomerActivityReportAsync(Guid customerId, DateRangeDto dateRange)
    {
        try
        {
            // Get customer details
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
            {
                throw new InvalidOperationException($"Customer with ID {customerId} not found");
            }

            // Get all orders for this customer within date range
            var customerOrders = await _orderRepository.GetByCustomerIdAsync(customerId);
            var ordersInRange = customerOrders.Where(o => 
                o.OrderDate >= dateRange.StartDate && 
                o.OrderDate <= dateRange.EndDate).ToList();

            // Get order statistics
            var totalOrders = ordersInRange.Count;
            var totalSpent = ordersInRange.Sum(o => o.TotalAmount);
            var avgOrderValue = totalOrders > 0 ? totalSpent / totalOrders : 0;
            
            // Get product preferences (top products ordered by this customer)
            var productPreferences = ordersInRange
                .SelectMany(o => o.OrderItems)
                .GroupBy(i => i.ProductSku)
                .Select(g => new ProductPreferenceDto
                {
                    ProductSku = g.Key,
                    ProductName = g.First().ProductName,
                    QuantityOrdered = g.Sum(i => i.Quantity),
                    TotalSpent = g.Sum(i => i.LineTotal)
                })
                .OrderByDescending(p => p.QuantityOrdered)
                .Take(5)
                .ToList();

            // Get order status breakdown
            var orderStatusBreakdown = ordersInRange
                .GroupBy(o => o.Status)
                .Select(g => new OrderStatusSummaryDto
                {
                    Status = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(o => o.TotalAmount)
                })
                .ToList();

            // Get recent orders
            var recentOrders = ordersInRange
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => _mapper.Map<OrderDto>(o))
                .ToList();

            return new CustomerActivityReportDto
            {
                CustomerId = customerId,
                CustomerName = customer.Name,
                StartDate = dateRange.StartDate,
                EndDate = dateRange.EndDate,
                TotalOrders = totalOrders,
                TotalSpent = totalSpent,
                AverageOrderValue = avgOrderValue,
                ProductPreferences = productPreferences,
                OrderStatusBreakdown = orderStatusBreakdown,
                RecentOrders = recentOrders,
                CreditLimit = customer.CreditLimit,
                CurrentBalance = customer.CurrentBalance,
                AvailableCredit = customer.AvailableCredit,
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating customer activity report for customer {CustomerId} for period {StartDate} to {EndDate}", 
                customerId, dateRange.StartDate, dateRange.EndDate);
            throw;
        }
    }

    public async Task<List<CustomerSegmentReportDto>> GetCustomerSegmentReportAsync()
    {
        try
        {
            // Get all customers
            var allCustomers = await _customerRepository.GetAllAsync();
            
            // Group by type and category
            var segments = allCustomers
                .GroupBy(c => new { c.CustomerType, c.CustomerCategory })
                .Select(g => new CustomerSegmentReportDto
                {
                    CustomerType = g.Key.CustomerType,
                    CustomerCategory = g.Key.CustomerCategory,
                    CustomerCount = g.Count(),
                    AverageCreditLimit = g.Average(c => c.CreditLimit),
                    TotalCreditExtended = g.Sum(c => c.CreditLimit),
                    TotalCurrentBalance = g.Sum(c => c.CurrentBalance),
                    AverageCurrentBalance = g.Average(c => c.CurrentBalance)
                })
                .OrderBy(s => s.CustomerType)
                .ThenBy(s => s.CustomerCategory)
                .ToList();

            return segments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating customer segment report");
            throw;
        }
    }

    public async Task<SalesPerformanceReportDto> GetSalesPerformanceReportAsync(DateRangeDto dateRange)
    {
        try
        {
            // Get all orders within date range
            var allOrders = await _orderRepository.GetAllAsync();
            var ordersInRange = allOrders.Where(o => 
                o.OrderDate >= dateRange.StartDate && 
                o.OrderDate <= dateRange.EndDate).ToList();

            // Calculate metrics
            var totalSales = ordersInRange.Where(o => o.Status != "Cancelled").Sum(o => o.TotalAmount);
            var totalOrders = ordersInRange.Where(o => o.Status != "Cancelled").Count();
            var cancelledOrders = ordersInRange.Count(o => o.Status == "Cancelled");
            var cancelRate = ordersInRange.Any() ? (decimal)cancelledOrders / ordersInRange.Count() : 0;
            
            // Calculate average fulfillment time for completed orders
            var completedOrders = ordersInRange.Where(o => o.Status == "Delivered" && o.DeliveredAt.HasValue);
            var avgFulfillmentTime = completedOrders.Any() 
                ? completedOrders.Average(o => (o.DeliveredAt!.Value - o.OrderDate).TotalHours) 
                : 0;

            // Get top customers
            var topCustomers = ordersInRange
                .Where(o => o.Status != "Cancelled")
                .GroupBy(o => o.CustomerId)
                .Select(g => new { CustomerId = g.Key, TotalSpent = g.Sum(o => o.TotalAmount) })
                .OrderByDescending(c => c.TotalSpent)
                .Take(5)
                .ToList();

            var customerIds = topCustomers.Select(c => c.CustomerId).ToList();
            var customers = (await _customerRepository.GetAllAsync())
                .Where(c => customerIds.Contains(c.Id))
                .ToList();

            var topCustomerDtos = topCustomers
                .Select(tc => {
                    var customer = customers.FirstOrDefault(c => c.Id == tc.CustomerId);
                    return new TopCustomerDto
                    {
                        CustomerId = tc.CustomerId,
                        CustomerName = customer?.Name ?? "Unknown Customer",
                        CustomerType = customer?.CustomerType ?? "Unknown",
                        TotalSpent = tc.TotalSpent
                    };
                })
                .ToList();

            return new SalesPerformanceReportDto
            {
                StartDate = dateRange.StartDate,
                EndDate = dateRange.EndDate,
                TotalSales = totalSales,
                TotalOrders = totalOrders,
                CancelledOrders = cancelledOrders,
                CancellationRate = cancelRate,
                AverageFulfillmentTimeHours = avgFulfillmentTime,
                TopCustomers = topCustomerDtos,
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sales performance report for period {StartDate} to {EndDate}", 
                dateRange.StartDate, dateRange.EndDate);
            throw;
        }
    }

    public async Task<List<SalesPerformanceByProductDto>> GetProductPerformanceReportAsync(DateRangeDto dateRange)
    {
        try
        {
            // Get all orders within date range
            var allOrders = await _orderRepository.GetAllAsync();
            var ordersInRange = allOrders.Where(o => 
                o.OrderDate >= dateRange.StartDate && 
                o.OrderDate <= dateRange.EndDate &&
                o.Status != "Cancelled").ToList();

            // Flatten to order items
            var orderItems = ordersInRange
                .SelectMany(o => o.OrderItems)
                .ToList();

            // Group by product
            var productPerformance = orderItems
                .GroupBy(i => i.ProductSku)
                .Select(g => new SalesPerformanceByProductDto
                {
                    ProductSku = g.Key,
                    ProductName = g.First().ProductName,
                    UnitsSold = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.LineTotal),
                    OrderCount = g.Select(i => i.OrderId).Distinct().Count(),
                    AverageUnitPrice = g.Sum(i => i.LineTotal) / g.Sum(i => i.Quantity)
                })
                .OrderByDescending(p => p.Revenue)
                .ToList();

            return productPerformance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating product performance report for period {StartDate} to {EndDate}", 
                dateRange.StartDate, dateRange.EndDate);
            throw;
        }
    }

    public async Task<List<SalesPerformanceByCustomerCategoryDto>> GetCustomerCategoryPerformanceReportAsync(DateRangeDto dateRange)
    {
        try
        {
            // Get all customers
            var allCustomers = await _customerRepository.GetAllAsync();
            
            // Get all orders within date range
            var allOrders = await _orderRepository.GetAllAsync();
            var ordersInRange = allOrders.Where(o => 
                o.OrderDate >= dateRange.StartDate && 
                o.OrderDate <= dateRange.EndDate &&
                o.Status != "Cancelled").ToList();

            // Join customers with orders
            var ordersByCategory = ordersInRange
                .Join(
                    allCustomers,
                    o => o.CustomerId,
                    c => c.Id,
                    (o, c) => new { Order = o, Customer = c }
                )
                .GroupBy(x => new { x.Customer.CustomerType, x.Customer.CustomerCategory })
                .Select(g => new SalesPerformanceByCustomerCategoryDto
                {
                    CustomerType = g.Key.CustomerType,
                    CustomerCategory = g.Key.CustomerCategory,
                    TotalSales = g.Sum(x => x.Order.TotalAmount),
                    OrderCount = g.Count(),
                    CustomerCount = g.Select(x => x.Customer.Id).Distinct().Count(),
                    AverageOrderValue = g.Sum(x => x.Order.TotalAmount) / g.Count()
                })
                .OrderByDescending(x => x.TotalSales)
                .ToList();

            return ordersByCategory;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating customer category performance report for period {StartDate} to {EndDate}", 
                dateRange.StartDate, dateRange.EndDate);
            throw;
        }
    }

    public async Task<CustomerOrderSummaryDto> GetCustomerOrderSummaryAsync(Guid customerId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            // Get customer orders within date range
            var allOrders = await _orderRepository.GetByCustomerIdAsync(customerId);
            var ordersInRange = allOrders.Where(o => 
                o.OrderDate >= fromDate && 
                o.OrderDate <= toDate).ToList();

            if (!ordersInRange.Any())
            {
                return new CustomerOrderSummaryDto
                {
                    TotalOrders = 0,
                    TotalValue = 0,
                    CancelledOrders = 0,
                    CompletedOrders = 0,
                    LastOrderDate = null
                };
            }

            var totalOrders = ordersInRange.Count;
            var totalValue = ordersInRange.Sum(o => o.TotalAmount);
            var cancelledOrders = ordersInRange.Count(o => o.Status == "Cancelled");
            var completedOrders = ordersInRange.Count(o => o.Status == "Delivered");
            var lastOrderDate = ordersInRange.OrderByDescending(o => o.OrderDate).FirstOrDefault()?.OrderDate;

            return new CustomerOrderSummaryDto
            {
                TotalOrders = totalOrders,
                TotalValue = totalValue,
                CancelledOrders = cancelledOrders,
                CompletedOrders = completedOrders,
                LastOrderDate = lastOrderDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating customer order summary for customer {CustomerId} for period {StartDate} to {EndDate}",
                customerId, fromDate, toDate);
            throw;
        }
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        // Placeholder: Replace with real analytics logic
        var allOrders = await _orderRepository.GetAllAsync();
        var allCustomers = await _customerRepository.GetAllAsync();
        var today = DateTime.UtcNow.Date;
        return new DashboardSummaryDto
        {
            TotalOrders = allOrders.Count(),
            TotalCustomers = allCustomers.Count(),
            TotalRevenue = allOrders.Sum(o => o.TotalAmount),
            ActiveCustomers = allCustomers.Count(c => c.IsActive),
            NewCustomers = allCustomers.Count(c => c.CreatedAt.Date == today),
            OrdersToday = allOrders.Count(o => o.OrderDate.Date == today),
            RevenueToday = allOrders.Where(o => o.OrderDate.Date == today).Sum(o => o.TotalAmount)
        };
    }

    public async Task<SalesPerformanceDto> GetSalesPerformanceAsync()
    {
        // Placeholder: Replace with real analytics logic
        var allOrders = await _orderRepository.GetAllAsync();
        var totalSales = allOrders.Sum(o => o.TotalAmount);
        var totalOrders = allOrders.Count();
        var avgOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;
        return new SalesPerformanceDto
        {
            TotalSales = totalSales,
            TotalOrders = totalOrders,
            AverageOrderValue = avgOrderValue,
            SalesTarget = 100000, // Example static target
            SalesTargetProgress = totalSales / 100000m * 100,
            Period = DateTime.UtcNow.Month
        };
    }

    public async Task<IEnumerable<TopProductDto>> GetTopProductsAsync()
    {
        // Placeholder: Replace with real analytics logic
        var allOrders = await _orderRepository.GetAllAsync();
        var orderItems = allOrders.SelectMany(o => o.OrderItems);
        var topProducts = orderItems
            .GroupBy(i => new { i.ProductSku, i.ProductName })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductSku,
                ProductName = g.Key.ProductName,
                QuantitySold = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.LineTotal)
            })
            .OrderByDescending(p => p.TotalRevenue)
            .Take(10)
            .ToList();
        return topProducts;
    }

    public async Task<CustomerInsightsDto> GetCustomerInsightsAsync()
    {
        // Placeholder: Replace with real analytics logic
        var allCustomers = (await _customerRepository.GetAllAsync()).ToList();
        var allOrders = (await _orderRepository.GetAllAsync()).ToList();
        var today = DateTime.UtcNow.Date;
        var newCustomers = allCustomers.Count(c => c.CreatedAt.Date == today);
        // returningCustomers, churnedCustomers, loyalCustomers are set to 0 as placeholders
        var returningCustomers = 0;
        var churnedCustomers = 0;
        var loyalCustomers = 0;
        var avgOrderValue = allOrders.Any() ? allOrders.Average(o => o.TotalAmount) : 0;
        var topCategory = allCustomers
            .GroupBy(c => c.CustomerCategory)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key ?? string.Empty;
        return new CustomerInsightsDto
        {
            TotalCustomers = allCustomers.Count,
            NewCustomersThisPeriod = newCustomers,
            ReturningCustomers = returningCustomers,
            AverageOrderValue = avgOrderValue,
            ChurnedCustomers = churnedCustomers,
            TopCustomerCategory = topCategory,
            LoyalCustomers = loyalCustomers
        };
    }
}
