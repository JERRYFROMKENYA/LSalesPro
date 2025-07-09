namespace SalesService.Application.DTOs;

public class DateRangeDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class SalesSummaryReportDto
{
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalSales { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal MaximumOrderValue { get; set; }
    public decimal MinimumOrderValue { get; set; }
    public int UniqueCustomers { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalDiscounts { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; }
    public List<OrderStatusSummaryDto> OrderStatusBreakdown { get; set; } = new List<OrderStatusSummaryDto>();
}

public class OrderStatusSummaryDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
}

public class DailySalesReportDto
{
    public DateTime Date { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal TotalSales { get; set; }
    public int CustomerCount { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public class MonthlySalesReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal TotalSales { get; set; }
    public int CustomerCount { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal GrowthPercentage { get; set; } // Compared to previous month
}

public class ProductSalesReportDto
{
    public string ProductSku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal AverageUnitPrice { get; set; }
    public decimal SalePercentage { get; set; } // Percentage of total sales
}

public class CustomerSalesReportDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerType { get; set; } = string.Empty;
    public string CustomerCategory { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalPurchases { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal AverageOrderValue { get; set; }
    public DateTime FirstPurchaseDate { get; set; }
    public DateTime LastPurchaseDate { get; set; }
    public DateTime LastOrderDate { get; set; }
}

public class ProductPreferenceDto
{
    public string ProductSku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int QuantityOrdered { get; set; }
    public decimal TotalSpent { get; set; }
}

public class CustomerActivityReportDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<CustomerOrderHistoryDto> OrderHistory { get; set; } = new List<CustomerOrderHistoryDto>();
    public List<OrderDto> RecentOrders { get; set; } = new List<OrderDto>();
    public List<ProductPreferenceDto> ProductPreferences { get; set; } = new List<ProductPreferenceDto>();
    public List<OrderStatusSummaryDto> OrderStatusBreakdown { get; set; } = new List<OrderStatusSummaryDto>();
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal AvailableCredit { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class CustomerOrderHistoryDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
}

public class TopProductByCustomerDto
{
    public string ProductSku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int QuantityPurchased { get; set; }
    public decimal TotalSpent { get; set; }
    public int OrderCount { get; set; } // Number of orders containing this product
}

public class CustomerSegmentReportDto
{
    public string CustomerType { get; set; } = string.Empty;
    public string CustomerCategory { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRevenuePerCustomer { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal AverageCreditLimit { get; set; }
    public decimal TotalCreditExtended { get; set; }
    public decimal TotalCurrentBalance { get; set; }
    public decimal AverageCurrentBalance { get; set; }
}

public class SalesPerformanceReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PreviousPeriodRevenue { get; set; }
    public decimal RevenueGrowth { get; set; } // Percentage
    public int TotalOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal CancellationRate { get; set; }
    public double AverageFulfillmentTimeHours { get; set; }
    public int PreviousPeriodOrders { get; set; }
    public decimal OrderGrowth { get; set; } // Percentage
    public int TotalCustomers { get; set; }
    public int PreviousPeriodCustomers { get; set; }
    public decimal CustomerGrowth { get; set; } // Percentage
    public decimal AverageOrderValue { get; set; }
    public decimal PreviousPeriodAverageOrderValue { get; set; }
    public decimal AverageOrderValueGrowth { get; set; } // Percentage
    public List<TopCustomerDto> TopCustomers { get; set; } = new List<TopCustomerDto>();
    public DateTime GeneratedAt { get; set; }
}

public class TopCustomerDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerType { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
}

public class SalesPerformanceByProductDto
{
    public string ProductSku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int UnitsSold { get; set; }
    public int CurrentPeriodQuantity { get; set; }
    public int PreviousPeriodQuantity { get; set; }
    public decimal QuantityGrowth { get; set; } // Percentage
    public decimal Revenue { get; set; }
    public decimal CurrentPeriodRevenue { get; set; }
    public decimal PreviousPeriodRevenue { get; set; }
    public decimal RevenueGrowth { get; set; } // Percentage
    public int OrderCount { get; set; }
    public decimal AverageUnitPrice { get; set; }
}

public class SalesPerformanceByCustomerCategoryDto
{
    public string CustomerType { get; set; } = string.Empty;
    public string CustomerCategory { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public decimal TotalSales { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal CurrentPeriodRevenue { get; set; }
    public decimal PreviousPeriodRevenue { get; set; }
    public decimal RevenueGrowth { get; set; } // Percentage
    public int CurrentPeriodOrders { get; set; }
    public int PreviousPeriodOrders { get; set; }
    public decimal OrderGrowth { get; set; } // Percentage
}

public class CustomerOrderSummaryDto
{
    public int TotalOrders { get; set; }
    public decimal TotalValue { get; set; }
    public int CancelledOrders { get; set; }
    public int CompletedOrders { get; set; }
    public DateTime? LastOrderDate { get; set; }
}
